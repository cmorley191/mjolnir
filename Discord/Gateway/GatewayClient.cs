using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Timers;
using Discord.Gateway.Models;
using Newtonsoft.Json;

namespace Discord {
    public class GatewayClient {
        public const string DefaultGateway = "wss://gateway.discord.gg/?v=6&encoding=json";

        private int HeartbeatInterval;
        private WebSocket socket;


        public void Connect() {
            using (var ws = new ClientWebSocket()) {
                var uri = new Uri(DefaultGateway);
                ws.ConnectAsync(uri, CancellationToken.None).Wait();
                socket = ws;

                HandleHello();
            }
        }

        private async void HandleHello() {
            var buffer = new ArraySegment<byte>(new byte[1024]);
            var raw = await socket.ReceiveAsync(buffer, CancellationToken.None);

            var json = Encoding.UTF8.GetString(buffer.Array, 0, raw.Count);
            var response = JsonConvert.DeserializeObject<Response>(json);

            Debug.Assert(response.OpCode == 10);

            var payload = (HelloPayload) response.DataPayload;
            Console.WriteLine($"Data Payload: {payload.HeartbeatInterval}");
        }
    }
}