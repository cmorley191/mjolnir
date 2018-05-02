using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Gateway;
using Discord.Gateway.Models;
using Discord.Gateway.Models.Payload;
using Newtonsoft.Json;
using NLog;

namespace Discord {
    public class GatewayClient {
        public const string DefaultGateway = "wss://gateway.discord.gg/?v=6&encoding=json";
        private const int MaxRetries = 3;
        private Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Tries to connect to the websocket to Discord and retries on errors.
        /// </summary>
        public void TryConnect() {
            for (var i = 0; i < MaxRetries; i++)
                try {
                    Connect();
                }
                catch (WebSocketException e) {
                    _log.Error(e, "Caight Exception in Websocket, restarting connection...");
                }
        }

        /// <summary>
        ///     Connects this instance.
        /// </summary>
        public void Connect() {
            using (var ws = new ClientWebSocket()) {

                // Connecting to the gateway
                var uri = new Uri(DefaultGateway);
                ws.ConnectAsync(uri, CancellationToken.None).Wait();
                _log.Info("Connected to Discord Socket!");

                // Handle the first message
                HandleHello(ws);

                //TODO: SEND OUR IDENTITY

                // deal with every other message
                while (ws.State == WebSocketState.Open) {
                    //TODO: QUEUEUP INCOMING SOCKET MESSAGES
                }
            }
        }

        /// <summary>
        ///     Handles the hello, which is the first response recieved from the
        ///     Discord servers.
        /// </summary>
        /// <exception cref="WebSocketException">Socket Out of Order: First message not OpCode 10, Hello.</exception>
        private async void HandleHello(WebSocket socket) {
            // read resposnse from the socket
            var buffer = new ArraySegment<byte>(new byte[1024]);
            var raw = await socket.ReceiveAsync(buffer, CancellationToken.None);

            // convert the data into 
            var json = Encoding.UTF8.GetString(buffer.Array, 0, raw.Count);
            var response = JsonConvert.DeserializeObject<Response>(json);

            // handle out of order messages
            if (response.OpCode != (int) OpCodeTypes.Hello)
                throw new WebSocketException("Socket Out of Order: First message not OpCode 10, Hello.");

            _log.Info("Server Hello Recivied, Gateway Established!");

            // start the heartbeat
            var payload = (HelloPayload) response.DataPayload;
            Heart.StartBeating(payload.HeartbeatInterval, socket, response.SequenceNumber);
        }
    }
}