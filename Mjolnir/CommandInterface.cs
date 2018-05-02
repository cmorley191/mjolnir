using Discord.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MjolnirCore;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Discord.Structures;
using System.Reflection;

namespace Mjolnir {
    public class CommandInterface {

        private HttpBotInterface http;
        private long listeningChannelId;

        public CommandInterface(HttpBotInterface http, long listeningChannelId) {
            this.http = http;
            this.listeningChannelId = listeningChannelId;

            startScanThread();
        }

        public delegate Task CommandListener(Message message);
        private IDictionary<string, IList<CommandListener>> Listeners = new Dictionary<string, IList<CommandListener>>();
        public const string UnknownCommandKey = "";

        public void AddListener(string command, CommandListener listener) {
            command = command.ToLower();
            if (Listeners.ContainsKey(command))
                Listeners[command].Add(listener);
            else
                Listeners[command] = new List<CommandListener>() { listener };
        }

        public void AddListeners(object commandObject) {
            Type objtype = commandObject.GetType();
            var methods = objtype.GetMethods();
            foreach (MethodInfo p in methods) {
                // for every property loop through all attributes
                foreach (Attribute a in p.GetCustomAttributes(true)) {
                    if (a is CommandAttr) {
                        var ca = a as CommandAttr;
                        foreach (string name in ca.Names)
                            AddListener(name, m => (Task)p.Invoke(commandObject, new object[] { m }));
                    }
                }
            }
        }

        private static readonly int MESSAGE_SCAN_DELAY_MS = 10000;

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
                                Debug.WriteLine($"Processing {newMessage.Id}");

                                var commandRegex = @"!([^\s]+)(.*)";
                                if (Regex.IsMatch(newMessage.Content, commandRegex)) {
                                    var groups = Regex.Match(newMessage.Content, commandRegex).Groups;
                                    var command = groups[1].ToString().ToLower();
                                    var arguments = groups[2].ToString().Trim();
                                    Debug.WriteLine($"Processing {command}: {arguments}");

                                    if (Listeners.ContainsKey(command)) {
                                        Debug.WriteLine($"Invoking {Listeners[command].Count} listeners");
                                        foreach (var listener in Listeners[command])
                                            await listener.Invoke(newMessage);
                                    } else {
                                        Debug.WriteLine($"Unknown command");
                                        if (Listeners.ContainsKey(UnknownCommandKey))
                                            foreach (var listener in Listeners[UnknownCommandKey])
                                                await listener.Invoke(newMessage);
                                    }
                                } else {
                                    Debug.WriteLine($"Found a non-command message.");
                                }
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
