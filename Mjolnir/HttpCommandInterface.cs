using Discord.Http;
using MjolnirCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Mjolnir {
    public class HttpCommandInterface : CommandInterface {

        private HttpBotInterface http;
        private long listeningChannelId;
        private static readonly int MESSAGE_SCAN_DELAY_MS = 10000;

        public HttpCommandInterface(HttpBotInterface http, long listeningChannelId) {
            this.http = http;
            this.listeningChannelId = listeningChannelId;

            startScanThread();
        }

        private void startScanThread() {
            var latestMessageIdOption = http.GetChannel(listeningChannelId).Result.LastMessageId;

            new Thread(async () => {
                while (true) {
                    if (latestMessageIdOption.IsNone()) {
                        Debug.WriteLine($"Checking for any messages at all in {listeningChannelId}");
                        latestMessageIdOption = (await http.GetChannel(listeningChannelId)).LastMessageId;
                    }

                    if (latestMessageIdOption.IsSome()) {
                        Debug.WriteLine($"Checking for new messages after {latestMessageIdOption.Value}");
                        var newMessages =
                            (await http.GetMessages(listeningChannelId, afterMessage: latestMessageIdOption.Value))
                            .OrderBy(message => message.Timestamp);

                        if (newMessages.Any()) {
                            Debug.WriteLine($"Processing {newMessages.Count()} new messages");
                            foreach (var newMessage in newMessages) {
                                await ProcessMessage(newMessage);
                            }

                            latestMessageIdOption = newMessages.Last().Id;
                        }
                    }

                    Thread.Sleep(MESSAGE_SCAN_DELAY_MS);
                }
            }).Start();
        }
    }
}
