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

namespace Discord {
    public class GatewayClient {
        public const string DefaultGateway = "wss://gateway.discord.gg/?v=6&encoding=json";
        private const int MaxRetries = 1;
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
                await Connect();
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
            Task.Run(() => SendMessagesAgent(ws));
            var readyResponseMonitor = new SemaphoreSlim(initialCount: 0);
            Task.Run(() => ReceiveMessagesAgent(ws, readyResponseMonitor));
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
            await readyResponseMonitor.WaitAsync();
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
            /// <summary>
            /// Monitored by the producer of the message to know when the message actually gets sent.
            /// </summary>
            public readonly SemaphoreSlim Monitor = new SemaphoreSlim(initialCount: 0);

            public Message(PayloadGenerator PayloadGenerator) {
                this.GetPayload = PayloadGenerator;
            }
        }

        private int? sequenceNumber = null;
        /// <summary>
        /// Must lock this to use sendQueue.
        /// </summary>
        private object sendQueueMonitor = new object();
        private Queue<Message> sendQueue = new Queue<Message>();
        /// <summary>
        /// Loop which sends messages that appear in the sendQueue.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private async Task SendMessagesAgent(WebSocket socket) {
            while (true) {
                Message message = null;
                while (message == null) {
                    lock (sendQueueMonitor) {
                        if (!sendQueue.Any()) {
                            Monitor.Wait(sendQueueMonitor);
                        } else {
                            message = sendQueue.Dequeue();
                        }
                    }
                }

                var messageData = message.GetPayload(sequenceNumber);
                var sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageData));
                await socket.SendAsync(sendBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);
                _log.Debug($"Sent: {messageData}");
                message.Monitor.Release();
            }
        }

        /// <summary>
        /// Add a message to the queue and wait for it to be sent.
        /// </summary>
        /// <param name="payloadGenerator"></param>
        /// <returns></returns>
        internal async Task SendMessage(PayloadGenerator payloadGenerator) {
            var message = new Message(payloadGenerator);
            lock (sendQueueMonitor) {
                sendQueue.Enqueue(message);
                Monitor.Pulse(sendQueueMonitor);
            }
            await message.Monitor.WaitAsync();
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
            } while (!receiveResult.EndOfMessage && messageBuilder.ToString() != "");

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
        /// <param name="readyResponseMonitor">This monitor will be pulsed when receiving a "Ready" message for the first time.</param>
        /// <returns></returns>
        private async Task ReceiveMessagesAgent(WebSocket socket, SemaphoreSlim readyResponseMonitor) {
            var receivedReadyReponse = false;

            while (true) {
                var response = await ReceiveMessage(socket);
                if (response.SequenceNumber.HasValue)
                    sequenceNumber = response.SequenceNumber;

                if (response.OpCode == (int)OpCodeTypes.Dispatch) {
                    // event dispatch
                    if (response.EventName == "READY") {
                        if (receivedReadyReponse) {
                            throw new WebSocketException("Received duplicate 0 Ready event.");
                        } else {
                            var ready = (ReadyPayload)response.DataPayload;

                            _log.Info($"Current User: {ready.CurrentUser.Username}");
                            _log.Info($"Accessible DMs: {ready.AssociatedDirectMessageChannels.Select(c => c.Name).ToSequenceString()}");
                            _log.Info($"Accessible Guilds: {ready.AssociatedGuilds.Select(g => g.Id).ToSequenceString()}");

                            sessionId = ready.SessionId;
                            readyResponseMonitor.Release();
                        }
                    } else if (messageHandlers.Keys.Any(k => k.name == response.EventName)) {
                        foreach (var handler in messageHandlers.Single(p => p.Key.name == response.EventName).Value)
                            await handler.Invoke(response.DataPayload);
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