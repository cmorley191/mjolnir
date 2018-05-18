using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Discord.Gateway.Models;
using Discord.Gateway.Models.Commands;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using static Discord.GatewayClient;

namespace Discord.Gateway {
    internal class Heart {
        private GatewayClient gateway;
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public Heart(int rate, GatewayClient gateway) {
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

        private object heartEventQueueMonitor = new object();
        private Queue<HeartEvent> heartEventQueue = new Queue<HeartEvent>();
        private async Task HeartAgent(int rate) {
            var heartbeatTimer = new System.Timers.Timer {
                Interval = rate,
                AutoReset = true
            };

            _logger.Info($"Starting Heartbeat with Interval={rate}");

            heartbeatTimer.Elapsed += TimeForABeat;
            heartbeatTimer.Start();

            TimeForABeat(null, null);

            while (true) {
                HeartEvent? heartEvent = null;
                while (heartEvent == null) {
                    lock (heartEventQueueMonitor) {
                        if (!heartEventQueue.Any()) {
                            Monitor.Wait(heartEventQueueMonitor);
                        } else {
                            heartEvent = heartEventQueue.Dequeue();
                        }
                    }
                }

                if (heartEvent.Value == HeartEvent.AckReceived) {
                    pendingTimedBeat = false;
                    pendingRequestedBeat = false;
                    _logger.Debug("Heartbeat Acknowledged");
                } else if (
                    (heartEvent.Value == HeartEvent.TimeForABeat && pendingTimedBeat)
                    || (heartEvent.Value == HeartEvent.BeatRequestedByServer && pendingRequestedBeat)) {
                    // TODO: Shutdown and reconnect
                    _logger.Debug("Heartbeat problem. Shutting down.");
                } else if (heartEvent.Value == HeartEvent.TimeForABeat) {
                    await Send();
                    pendingTimedBeat = true;
                } else if (heartEvent.Value == HeartEvent.BeatRequestedByServer) {
                    heartbeatTimer.Stop();
                    await Send();
                    pendingRequestedBeat = true;
                    heartbeatTimer.Start();
                }
            }
        }

        public void AckReceived() {
            lock (heartEventQueueMonitor) {
                heartEventQueue.Enqueue(HeartEvent.AckReceived);
                Monitor.Pulse(heartEventQueueMonitor);
            }
        }

        public void BeatRequested() {
            lock (heartEventQueueMonitor) {
                heartEventQueue.Enqueue(HeartEvent.BeatRequestedByServer);
                Monitor.Pulse(heartEventQueueMonitor);
            }
        }

        /// <summary>
        /// Timers the on elapsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void TimeForABeat(object sender, ElapsedEventArgs e) {
            lock (heartEventQueueMonitor) {
                heartEventQueue.Enqueue(HeartEvent.TimeForABeat);
                Monitor.Pulse(heartEventQueueMonitor);
            }
        }

        private async Task Send() {
            _logger.Debug("Sending Heartbeat!");
            PayloadGenerator message = (sequenceNumber) => JsonConvert.SerializeObject(new HeartBeatCommand {
                Sequence = sequenceNumber
            });

            await gateway.SendMessage(message);
        }

    }
}