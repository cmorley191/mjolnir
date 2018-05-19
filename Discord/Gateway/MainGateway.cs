using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
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
using Discord.Structures;

namespace Discord.Gateway {
    public class MainGateway : Gateway {
        public const string DefaultGateway = "wss://gateway.discord.gg/?v=6&encoding=json";

        private readonly string authorizationToken;

        public MainGateway(string authorizationToken = null) {
            this.authorizationToken =
                authorizationToken
                ?? Environment.GetEnvironmentVariable("DISCORD_TOKEN")
                ?? throw new ArgumentException("No authorization token provided.");
        }

        private Heart heart = null;
        internal int? sequenceNumber = null;

        private class MainGatewayHeart : Heart {
            private MainGateway gateway;

            public MainGatewayHeart(int interval, MainGateway gateway) : base(interval, gateway) {
                this.gateway = gateway;
            }

            internal override string ProduceHeartbeatMessage() => JsonConvert.SerializeObject(new HeartBeatCommand() {
                OpCode = (int)MainOpCodeTypes.HeartbeatMsg,
                Sequence = gateway.sequenceNumber
            });
        }

        private User currentUser = null;
        public User CurrentUser => currentUser;
        private string sessionId = null;
        private VoiceGateway voiceGateway = null;
        public VoiceGateway VoiceGateway => voiceGateway;

        private TaskCompletionSource<ReadyPayload> readyResponseSource = new TaskCompletionSource<ReadyPayload>();

        public override async Task Connect() {
            // Connect to the gateway
            socket = new ClientWebSocket();
            var uri = new Uri(DefaultGateway);
            await socket.ConnectAsync(uri, CancellationToken.None);
            _log.Info("Connected to a Discord socket.");

            // Receive 10 Hello
            var response = await ReceiveMessage();
            if (response.OpCode != (int)MainOpCodeTypes.Hello)
                throw new WebSocketException("Socket Out of Order: First message not OpCode 10, Hello.");
            var hello = response.DeserializeDataPayload<HelloPayload>();

            // Set up messaging systems
            SendMessagesAgentAsync();
            ReceiveMessagesAgentAsync();
            _log.Debug("Started messaging systems.");

            // Send 1 Heartbeat (and continue to do so)
            heart = new MainGatewayHeart(hello.HeartbeatInterval, this);
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

            currentUser = ready.CurrentUser;
            sessionId = ready.SessionId;

            _log.Info($"Fully connected.");
        }

        public delegate Task EventHandler(dynamic payload);
        /// <summary>
        /// Users of this API - handlers for type 0 (dispatch) messages from the server.
        /// </summary>
        private IDictionary<EventType, IList<EventHandler>> messageHandlers = new Dictionary<EventType, IList<EventHandler>>();

        internal override Task HandleResponse(Response response) {
            if (response.SequenceNumber.HasValue)
                sequenceNumber = response.SequenceNumber;

            if (response.OpCode == (int)MainOpCodeTypes.Dispatch) {
                // event dispatch
                if (response.EventName == EventType.Ready.Name) {
                    var ready = response.DeserializeDataPayload<ReadyPayload>();
                    readyResponseSource.SetResult(ready);
                } else if (response.EventName == EventType.VoiceStateUpdate.Name) {
                    var voiceStateUpdate = response.DeserializeDataPayload<VoiceState>();
                    if (voiceStateUpdate.UserId == currentUser.Id && voiceGateway != null)
                        voiceGateway.InitialVoiceStateUpdateSource.SetResult(voiceStateUpdate);
                    else
                        _log.Info($"User {voiceStateUpdate.UserId} changed voice channel.");
                } else if (response.EventName == EventType.VoiceServerUpdate.Name) {
                    var voiceServerUpdate = response.DeserializeDataPayload<VoiceServerUpdatePayload>();
                    if (voiceGateway != null)
                        voiceGateway.InitialVoiceServerUpdateSource.SetResult(voiceServerUpdate);

                } else if (messageHandlers.Keys.Any(k => k.Name == response.EventName)) {
                    // found a user-specified handler
                    var eventType = messageHandlers.Keys.Single(handledEventType => handledEventType.Name == response.EventName);
                    foreach (var handler in messageHandlers[eventType])
                        handler.Invoke(response.DeserializeDataPayload(eventType.PayloadType)).ConfigureAwait(continueOnCapturedContext: false);

                } else
                    _log.Info($"Unhandled event type: {response.EventName}");
            } else if (response.OpCode == (int)MainOpCodeTypes.HeartbeatMsg) {
                // server requesting a heartbeat
                heart.BeatRequested();
            } else if (response.OpCode == (int)MainOpCodeTypes.Reconnect) {
                // TODO: client must reconnect
            } else if (response.OpCode == (int)MainOpCodeTypes.InvalidSession) {
                // could receive this for a lot of reasons, TODO: some of them can be handled
                throw new WebSocketException("Received 9 Invalid Session");
            } else if (response.OpCode == (int)MainOpCodeTypes.Hello) {
                // should never happen
                throw new WebSocketException("Received 10 Hello after already connected.");
            } else if (response.OpCode == (int)MainOpCodeTypes.HeartbeatAck) {
                // forward on to the heart
                heart.AckReceived();
            } else {
                // invalid receiving opcode
                throw new WebSocketException($"Received unknown message from server: opcode {response.OpCode}");
            }

            return Task.CompletedTask;
        }

        public void AddEventHandler(EventType eventType, EventHandler handler) {
            if (messageHandlers.ContainsKey(eventType))
                messageHandlers[eventType].Add(handler);
            else
                messageHandlers[eventType] = new List<EventHandler>() { handler };
        }

        public async Task ConnectToVoice(Channel voiceChannel) {
            voiceGateway = new VoiceGateway(this, voiceChannel);
            await voiceGateway.TryConnect();
        }
    }
}