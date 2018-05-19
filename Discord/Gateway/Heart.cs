using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using Discord.Gateway.Models;
using Discord.Gateway.Models.Commands;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using static Discord.Gateway.Gateway;

namespace Discord.Gateway {
    internal abstract class Heart {
        private Gateway gateway;
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public Heart(int rate, Gateway gateway) {
            this.gateway = gateway;

            Task.Run(() => HeartAgent(rate));
        }

        private enum HeartEvent {
            TimeForABeat,
            AckReceived,
            BeatRequestedByServer
        }
        private bool pendingTimedBeat = false;
        private bool pendingRequestedBeat = false;

        private BufferBlock<HeartEvent> heartEventQueue = new BufferBlock<HeartEvent>();
        private async Task HeartAgent(int rate) {
            var heartbeatTimer = new System.Timers.Timer {
                Interval = rate,
                AutoReset = true
            };

            _logger.Info($"{GetType().Name} beating with interval: {rate}ms");

            heartbeatTimer.Elapsed += TimeForABeat;
            heartbeatTimer.Start();

            TimeForABeat(null, null);

            while (true) {
                var heartEvent = await heartEventQueue.ReceiveAsync();

                if (heartEvent == HeartEvent.AckReceived) {
                    pendingTimedBeat = false;
                    pendingRequestedBeat = false;
                    _logger.Debug($"{GetType().Name} beat Acknowledged");
                } else if (
                    (heartEvent == HeartEvent.TimeForABeat && pendingTimedBeat)
                    || (heartEvent == HeartEvent.BeatRequestedByServer && pendingRequestedBeat)) {
                    // TODO: Shutdown and reconnect
                    _logger.Debug($"{GetType().Name} beat problem. Shut down.");
                }
                if (heartEvent == HeartEvent.TimeForABeat) {
                    await SendBeat();
                    pendingTimedBeat = true;
                } else if (heartEvent == HeartEvent.BeatRequestedByServer) {
                    heartbeatTimer.Stop();
                    await SendBeat();
                    pendingRequestedBeat = true;
                    heartbeatTimer.Start();
                }
            }
        }

        public void AckReceived() {
            heartEventQueue.Post(HeartEvent.AckReceived);
        }

        public void BeatRequested() {
            heartEventQueue.Post(HeartEvent.BeatRequestedByServer);
        }

        /// <summary>
        /// Timers the on elapsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void TimeForABeat(object sender, ElapsedEventArgs e) {
            heartEventQueue.Post(HeartEvent.TimeForABeat);
        }

        private async Task SendBeat() {
            _logger.Debug($"{GetType().Name} sending beat!");
            await gateway.SendMessage(ProduceHeartbeatMessage);
        }

        internal abstract string ProduceHeartbeatMessage();
    }
}