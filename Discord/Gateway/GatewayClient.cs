using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Discord.Gateway;
using Discord.Gateway.Models;
using Discord.Gateway.Models.Messages;
using Discord.Gateway.Models.Payload;
using Newtonsoft.Json;
using NLog;
using MjolnirCore.Extensions;

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
                } catch (WebSocketException e) {
                    _log.Error(e, "Caight Exception in Websocket, restarting connection...");
                }
        }

        /// <summary>
        ///     Connects this instance.
        /// </summary>
        public async Task Connect() {
            using (var ws = new ClientWebSocket()) {

                // Connecting to the gateway
                var uri = new Uri(DefaultGateway);
                await ws.ConnectAsync(uri, CancellationToken.None);
                _log.Info("Connected to Discord Socket!");

                // Handle the first message
                await HandleHello(ws);

                await SendIdentity(ws);

                // deal with every other message
                while (ws.State == WebSocketState.Open) {

                }
            }
        }

        /// <summary>
        ///     Handles the hello, which is the first response recieved from the
        ///     Discord servers.
        /// </summary>
        /// <exception cref="WebSocketException">Socket Out of Order: First message not OpCode 10, Hello.</exception>
        private async Task HandleHello(WebSocket socket) {
            // read resposnse from the socket
            var buffer = new ArraySegment<byte>(new byte[1024]);
            var raw = await socket.ReceiveAsync(buffer, CancellationToken.None);

            // convert the data into 
            var json = Encoding.UTF8.GetString(buffer.Array, 0, raw.Count);
            var response = JsonConvert.DeserializeObject<Response>(json);

            // handle out of order messages
            if (response.OpCode != (int)OpCodeTypes.Hello)
                throw new WebSocketException("Socket Out of Order: First message not OpCode 10, Hello.");

            _log.Info("Server Hello Recivied, Gateway Established!");

            // start the heartbeat
            var payload = (HelloPayload)response.DataPayload;
            Heart.StartBeating(payload.HeartbeatInterval, socket, response.SequenceNumber);
        }

        private async Task SendIdentity(WebSocket socket) {
            var message = new IdentifyMessage {
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

            var sendJson = JsonConvert.SerializeObject(message);
            var sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(sendJson));

            _log.Debug("Sending Identity");
            await socket.SendAsync(sendBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);

            // receive ready event response

            var buffer = new ArraySegment<byte>(new byte[1024]);
            var raw = await socket.ReceiveAsync(buffer, CancellationToken.None);

            var json = Encoding.UTF8.GetString(buffer.Array, 0, raw.Count);
            var response = JsonConvert.DeserializeObject<Response>(json);

            if (response.OpCode != (int)OpCodeTypes.Dispatch)
                throw new WebSocketException("Socket Out of Order: Identity response not a ready event");

            var payload = (ReadyEvent)response.DataPayload;
            _log.Info($"Current User: {payload.CurrentUser.Username}");
            _log.Info($"Accessible DMs: {payload.AssociatedDirectMessageChannels.Select(c => c.Name).ToSequenceString()}");
            _log.Info($"Accessible Guilds: {payload.AssociatedGuilds.Select(g => g.Id).ToSequenceString()}");
        }
    }
}