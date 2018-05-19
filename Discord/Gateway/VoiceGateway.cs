using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord.Gateway.Models;
using Discord.Gateway.Models.Commands;
using Discord.Gateway.Models.Payload;
using Discord.Gateway.Models.Payload.Events;
using Discord.Structures;
using MjolnirCore.Extensions;
using Newtonsoft.Json;

namespace Discord.Gateway {
    public class VoiceGateway : Gateway {

        private readonly MainGateway mainGateway;
        private readonly Channel voiceChannel;

        internal VoiceGateway(MainGateway mainGateway, Channel voiceChannel) {
            this.mainGateway = mainGateway;
            this.voiceChannel = voiceChannel;
        }

        internal TaskCompletionSource<VoiceState> InitialVoiceStateUpdateSource = new TaskCompletionSource<VoiceState>();
        internal TaskCompletionSource<VoiceServerUpdatePayload> InitialVoiceServerUpdateSource = new TaskCompletionSource<VoiceServerUpdatePayload>();

        private Heart heart = null;
        private class VoiceGatewayHeart : Heart {
            public VoiceGatewayHeart(int interval, Gateway gateway) : base(interval, gateway) { }

            private Random rng = new Random();
            internal override string ProduceHeartbeatMessage() => JsonConvert.SerializeObject(new HeartBeatCommand() {
                OpCode = (int)VoiceOpCodeTypes.Heartbeat,
                Sequence = rng.Next()
            });
        }

        private UdpClient udp;

        private string sessionId = null;
        private string token = null;
        private int? ssrc = null;
        private byte[] encryptionKey = null;

        private TaskCompletionSource<VoiceReadyPayload> readyResponseSource = new TaskCompletionSource<VoiceReadyPayload>();
        private TaskCompletionSource<VoiceSessionDescriptionPayload> sessionDescriptionResponseSource = new TaskCompletionSource<VoiceSessionDescriptionPayload>();

        public override async Task Connect() {
            // Send 4 Voice State Update
            var voiceStateUpdateRequest = new VoiceStateUpdateCommand {
                Data = {
                    GuildId = voiceChannel.GuildId.Value,
                    ChannelId = voiceChannel.Id,
                    MuteSelf = false,
                    DeafenSelf = false,
                }
            };
            await mainGateway.SendMessage(JsonConvert.SerializeObject(voiceStateUpdateRequest));

            // Receive 0 Voice State Update and 0 Voice Server Update
            var voiceStateUpdate = await InitialVoiceStateUpdateSource.Task;
            var voiceServerUpdate = await InitialVoiceServerUpdateSource.Task;
            Debug.Assert(voiceStateUpdate.GuildId.Value == voiceServerUpdate.GuildId);
            _log.Info("Received voice update messages.");

            sessionId = voiceStateUpdate.SessionId;
            token = voiceServerUpdate.VoiceConnectionToken;

            var endpoint = voiceServerUpdate.Endpoint;
            if (endpoint.EndsWith(":80"))
                endpoint = endpoint.Substring(0, endpoint.Length - ":80".Length);

            // Connect to the gateway
            socket = new ClientWebSocket();
            var uri = new Uri("wss://" + endpoint + "/?v=3");
            await socket.ConnectAsync(uri, CancellationToken.None);
            _log.Info($"Connected to a Discord voice socket: {endpoint}");

            // Set up messaging systems
            SendMessagesAgentAsync();
            _log.Debug("Started outgoing message system.");

            // Send 0 Identify
            var identify = new VoiceIdentifyCommand {
                Data = {
                        SessionId = voiceStateUpdate.SessionId,
                        UserId = voiceStateUpdate.UserId.ToString(),
                        ServerId = voiceServerUpdate.GuildId.ToString(),
                        VoiceConnectionToken = voiceServerUpdate.VoiceConnectionToken,
                    }
            };
            await SendMessage(JsonConvert.SerializeObject(identify));
            _log.Debug("Sent identity.");

            // Receive 8 Hello
            var response = await ReceiveMessage();
            if (response.OpCode != (int)VoiceOpCodeTypes.Hello)
                throw new WebSocketException("Socket Out of Order: First message not OpCode 8, Hello.");
            var hello = response.DeserializeDataPayload<HelloPayload>();
            _log.Debug("Received 8 Hello.");

            ReceiveMessagesAgentAsync();
            _log.Debug("Started incoming message system.");

            // Receive 2 Ready
            var ready = await readyResponseSource.Task;
            ssrc = ready.SSRC;
            Debug.Assert(ready.SupportedEncryptionModes.Contains("xsalsa20_poly1305"));

            // Send 3 Heartbeat (and continue to do so)
            heart = new VoiceGatewayHeart((int)(hello.HeartbeatInterval * 0.75), this);
            _log.Debug("Started heartbeat.");

            // Connect UDP
            udp = new UdpClient(11000);
            udp.Connect(endpoint, ready.UdpPort);
            _log.Info($"Connected to udp port {ready.UdpPort}.");
            ReceiveDataAgent();

            // IP Discovery
            string ip4Address;
            using (WebClient client = new WebClient()) {
                var ip4MeHtml = client.DownloadString("http://ip4.me");
                ip4Address = Regex.Match(ip4MeHtml, @"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+").Value;
                _log.Info($"Discovered my ip address: {ip4Address}");
            }

            // Send 1 Select Protocol
            var selectProtocol = new VoiceSelectProtocolCommand {
                Data = {
                    Data = {
                        ClientExternalIpAddress = ip4Address,
                        ClientPort = 11000,
                        SelectedEncryptionMode = "xsalsa20_poly1305" // the only protocol allowed by Discord
                    }
                }
            };
            await SendMessage(JsonConvert.SerializeObject(selectProtocol));
            _log.Info("Sent select protocol.");

            // Receive 4 Session Description
            var sessionDescription = await sessionDescriptionResponseSource.Task;
            Debug.Assert(sessionDescription.EncryptionMode == "xsalsa20_poly1305");
            encryptionKey = sessionDescription.EncryptionKey;

            _log.Info($"Fully connected.");
        }

        private async Task ReceiveDataAgent() {
            _log.Debug("Awaiting datagrams.");
            while (true) {
                var datagram = await udp.ReceiveAsync();
                _log.Debug($"Received datagram from {datagram.RemoteEndPoint.Address}:{datagram.RemoteEndPoint.Port}");
                _log.Debug($"Datagram: {datagram.Buffer.ToSequenceString()}");
            }
        }

        internal override Task HandleResponse(Response response) {
            if (response.OpCode == (int)VoiceOpCodeTypes.Ready) {
                var ready = response.DeserializeDataPayload<VoiceReadyPayload>();
                readyResponseSource.SetResult(ready);
            } else if (response.OpCode == (int)VoiceOpCodeTypes.Heartbeat) {
                // server requesting a heartbeat
                heart.BeatRequested();
            } else if (response.OpCode == (int)VoiceOpCodeTypes.SessionDescription) {
                var sessionDescription = response.DeserializeDataPayload<VoiceSessionDescriptionPayload>();
                sessionDescriptionResponseSource.SetResult(sessionDescription);
            } else if (response.OpCode == (int)VoiceOpCodeTypes.Speaking) {
                // TODO: update speaking status / allow user handlers for this
            } else if (response.OpCode == (int)VoiceOpCodeTypes.Resumed) {
                // TODO: voice gateway resuming
            } else if (response.OpCode == (int)VoiceOpCodeTypes.Hello) {
                // should never happen
                throw new WebSocketException("Received 8 Hello after already connected.");
            } else if (response.OpCode == (int)VoiceOpCodeTypes.HeartbeatAck) {
                // forward on to the heart
                heart.AckReceived();
            } else if (response.OpCode == (int)VoiceOpCodeTypes.ClientDisconnect) {
                // API description: a client has disconnected from the voice channel
                // Not sure exactly what this kind of response is, though.
                // TODO
            } else {
                // invalid receiving opcode
                throw new WebSocketException($"Received unknown message from server: opcode {response.OpCode}");
            }

            return Task.CompletedTask;
        }
    }
}
