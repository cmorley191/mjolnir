using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Timers;
using Discord.Gateway.Models;
using Discord.Gateway.Models.Messages;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;

namespace Discord.Gateway {
    internal class Heart {
        public static int? Sequence;
        private static WebSocket _socket;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Starts the beating.
        /// </summary>
        /// <param name="rate">The rate.</param>
        /// <param name="socket">The socket.</param>
        /// <param name="sequence">The sequence.</param>
        public static void StartBeating(int rate, WebSocket socket, int? sequence) {
            _socket = socket;
            Sequence = sequence;

            var timer = new System.Timers.Timer {
                Interval = rate,
                AutoReset = true
            };

            _logger.Info($"Starting Heartbeat with Interval={rate}");

            timer.Elapsed += TimerOnElapsed;
            timer.Start();
        }

        /// <summary>
        /// Sends the beat.
        /// </summary>
        /// <exception cref="WebSocketException">Wrong Response Recieved</exception>
        public static void SendBeat() {
            // create the message
            var message = new HeartBeatMessage {
                Sequence = Sequence
            };

            // send the heartbeat
            var sendJson = JsonConvert.SerializeObject(message);
            var sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(sendJson));

            _logger.Debug("Sending Heartbeat!");
            _socket.SendAsync(sendBuffer, WebSocketMessageType.Binary, true, CancellationToken.None).Wait();

            // recieve the response
            var recieveBuffer = new ArraySegment<byte>(new byte[1024]);
            var raw = _socket.ReceiveAsync(recieveBuffer, CancellationToken.None).Result;

            // parse the json
            var responseJson = Encoding.UTF8.GetString(recieveBuffer.Array, 0, raw.Count);
            var response = JsonConvert.DeserializeObject<Response>(responseJson);

            // confirm the ack
            if (response.OpCode != (int) OpCodeTypes.HeartbeatAck) {
                var err = new WebSocketException("Wrong Response Recieved, Assuming Discord has been by aliens!");
                _logger.Warn(err);

                throw err;
            }

			_logger.Debug("Heartbeat Acknowledged");
        }

        /// <summary>
        /// Timers the on elapsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private static void TimerOnElapsed(object sender, ElapsedEventArgs e) {
            SendBeat();
        }
    }
}