using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Discord.Gateway;
using Discord.Gateway.Models;
using Discord.Gateway.Models.Commands;
using Discord.Gateway.Models.Payload;
using Newtonsoft.Json;
using NLog;
using MjolnirCore.Extensions;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json.Linq;
using Discord.Gateway.Models.Payload.Events;
using System.Threading.Tasks.Dataflow;

namespace Discord {
    public class GatewayClient {
        public const string DefaultGateway = "wss://gateway.discord.gg/?v=6&encoding=json";
        private const int MaxRetries = 3;
        private Logger _log = LogManager.GetCurrentClassLogger();
        private readonly string authorizationToken;

        public GatewayClient(string authorizationToken = null) {
            this.authorizationToken =
                authorizationToken
                ?? Environment.GetEnvironmentVariable("DISCORD_TOKEN")
                ?? throw new ArgumentException("No authorization token provided.");
        }

        /// <summary>
        /// Tries to connect to the websocket to Discord and retries on errors.
        /// </summary>
        public async Task TryConnect() {
            for (var i = 0; i < MaxRetries; i++)
                try {
                    await Connect();
                    return;
                } catch (Exception e) {
                    if (i >= MaxRetries)
                        throw e;
                    else
                        _log.Debug("Connection failed. Retrying...");
                }
        }

        private Heart heart = null;
        private string sessionId = null;

        /// <summary>
        ///     Connects this instance.
        /// </summary>
        public async Task Connect() {
            var ws = new ClientWebSocket();

            // Connect to the gateway
            var uri = new Uri(DefaultGateway);
            await ws.ConnectAsync(uri, CancellationToken.None);
            _log.Info("Connected to Discord Socket!");

            // Receive 10 Hello
            var response = await ReceiveMessage(ws);
            if (response.OpCode != (int)OpCodeTypes.Hello)
                throw new WebSocketException("Socket Out of Order: First message not OpCode 10, Hello.");
            var hello = (HelloPayload)response.DataPayload;

            // Set up messaging systems
            SendMessagesAgentAsync(ws);
            var readyResponseSource = new TaskCompletionSource<ReadyPayload>();
            ReceiveMessagesAgentAsync(ws, readyResponseSource);
            _log.Debug("Started messaging systems.");

            // Send 1 Heartbeat (and continue to do so)
            heart = new Heart(hello.HeartbeatInterval, this);
            _log.Debug("Started heartbeat.");

            // Send 2 Identify
            var identify = new IdentifyCommand {
                Data = {
                        AuthenticationToken = authorizationToken,
                        ConnectionProperties = {
                            OperatingSystemName = "windows",
                            BrowserName = "mjolnir",
                            DeviceName = "mjolnir",
                        },
                        AllowCompression = false,
                    }
            };
            await SendMessage(JsonConvert.SerializeObject(identify));
            _log.Debug("Sent identity.");

            // Receive 0 Ready
            var ready = await readyResponseSource.Task;

            _log.Info($"Current User: {ready.CurrentUser.Username}");
            _log.Info($"Accessible DMs: {ready.AssociatedDirectMessageChannels.Select(c => c.Name).ToSequenceString()}");
            _log.Info($"Accessible Guilds: {ready.AssociatedGuilds.Select(g => g.Id).ToSequenceString()}");

            sessionId = ready.SessionId;

            _log.Info($"Fully connected.");
        }

        internal delegate string PayloadGenerator(int? sequenceNumber);
        /// <summary>
        /// A message to be sent to the server.
        /// </summary>
        internal class Message {
            /// <summary>
            /// A method for generating the message - the sequence number must be put into the message just before sending it (in case it changes in the meantime),
            /// so this can't be a constant string for applicable messages (heartbeat, resume).
            /// </summary>
            public readonly PayloadGenerator GetPayload;
            public readonly TaskCompletionSource<bool> MessageSent = new TaskCompletionSource<bool>();

            public Message(PayloadGenerator PayloadGenerator) {
                this.GetPayload = PayloadGenerator;
            }
        }

        private int? sequenceNumber = null;
        private BufferBlock<Message> sendQueue = new BufferBlock<Message>();
        /// <summary>
        /// Loop which sends messages that appear in the sendQueue.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private async Task SendMessagesAgentAsync(WebSocket socket) {
            while (true) {
                var message = await sendQueue.ReceiveAsync();
                var messageData = message.GetPayload(sequenceNumber);
                var sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageData));
                _log.Debug($"Sending: {messageData}");
                await socket.SendAsync(sendBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);
                _log.Debug($"Sent: {messageData}");
                message.MessageSent.SetResult(true);
            }
        }

        /// <summary>
        /// Add a message to the queue and wait for it to be sent.
        /// </summary>
        /// <param name="payloadGenerator"></param>
        /// <returns></returns>
        internal Task SendMessage(PayloadGenerator payloadGenerator) {
            var message = new Message(payloadGenerator);
            sendQueue.Post(message);
            return message.MessageSent.Task;
        }

        /// <summary>
        /// Add a message to the queue and wait for it to be sent.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        internal Task SendMessage(string payload) => SendMessage(_ => payload);

        /// <summary>
        /// Receive and parse a single message and return it.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private async Task<Response> ReceiveMessage(WebSocket socket) {
            var messageBuilder = new StringBuilder();
            WebSocketReceiveResult receiveResult;
            do {
                receiveResult = null;

                var buffer = new ArraySegment<byte>(new byte[1024]);
                receiveResult = await socket.ReceiveAsync(buffer, CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer.Array, 0, receiveResult.Count);
                messageBuilder.Append(message);
            } while (!receiveResult.EndOfMessage || messageBuilder.ToString().Trim() == "");

            _log.Debug($"Message: {messageBuilder.ToString()}");

            return JsonConvert.DeserializeObject<Response>(messageBuilder.ToString());
        }

        public delegate Task EventHandler(dynamic payload);
        /// <summary>
        /// Users of this API - handlers for type 0 (dispatch) messages from the server.
        /// </summary>
        private IDictionary<EventType, IList<EventHandler>> messageHandlers = new Dictionary<EventType, IList<EventHandler>>();

        /// <summary>
        /// Loop which receives messages and passes them off to the appropriate destination (usually a handler).
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="readyResponseSource">Source for receiving the single Ready response to the Identify command.</param>
        /// <returns></returns>
        private async Task ReceiveMessagesAgentAsync(WebSocket socket, TaskCompletionSource<ReadyPayload> readyResponseSource) {
            while (true) {
                var response = await ReceiveMessage(socket);
                if (response.SequenceNumber.HasValue)
                    sequenceNumber = response.SequenceNumber;

                if (response.OpCode == (int)OpCodeTypes.Dispatch) {
                    // event dispatch
                    if (response.EventName == "READY") {
                        var ready = (ReadyPayload)response.DataPayload;
                        readyResponseSource.SetResult(ready);
                    } else if (messageHandlers.Keys.Any(k => k.name == response.EventName)) {
                        foreach (var handler in messageHandlers.Single(p => p.Key.name == response.EventName).Value)
                            handler.Invoke(response.DataPayload).ConfigureAwait(continueOnCapturedContext: false);
                    } else {
                        _log.Info($"Unhandled event type: {response.EventName}");
                    }
                } else if (response.OpCode == (int)OpCodeTypes.HeartbeatMsg) {
                    // server requesting a heartbeat
                    heart.BeatRequested();
                } else if (response.OpCode == (int)OpCodeTypes.Reconnect) {
                    // TODO: client must reconnect
                } else if (response.OpCode == (int)OpCodeTypes.InvalidSession) {
                    // could receive this for a lot of reasons, TODO: some of them can be handled
                    throw new WebSocketException("Received 9 Invalid Session");
                } else if (response.OpCode == (int)OpCodeTypes.Hello) {
                    // should never happen
                    throw new WebSocketException("Received 10 Hello after already connected.");
                } else if (response.OpCode == (int)OpCodeTypes.HeartbeatAck) {
                    // forward on to the heart
                    heart.AckReceived();
                } else {
                    // invalid receiving opcode
                    throw new WebSocketException($"Received unknown message from server: opcode {response.OpCode}");
                }
            }
        }

        public void AddEventHandler(EventType eventType, EventHandler handler) {
            if (messageHandlers.ContainsKey(eventType))
                messageHandlers[eventType].Add(handler);
            else
                messageHandlers[eventType] = new List<EventHandler>() { handler };
        }
    }
}