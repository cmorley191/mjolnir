using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Http;
using Discord.Structures;
using DotNetEnv;
using MjolnirCore;
using MjolnirCore.Extensions;
using System.Reflection;

namespace Mjolnir {
    internal class Program {
        private static void Main(string[] args) {
            Env.Load(Path.Combine(EnvironmentHelper.SolutionFolderPath, ".env"));

            gatewayDemo();
        }

        private static void gatewayDemo() {
            var gate = new GatewayClient();
            Task.Run(() => gate.TryConnect());
                
            Console.WriteLine("DISCORD GATEWAY SERVICE!");
            Console.WriteLine("Press Enter to Terminate: \n\n");
            Console.ReadKey();


            Console.WriteLine("SIGTERM RECIEVED, SHUTTING DOWN GRACEFULLY!");
        }

        private static void commandDemo() {
            var http = new HttpBotInterface();

            var guilds = http.GetAccessibleGuilds().Result;
            var guild = guilds.Single(g => g.Name == "Anime_NSFW");
            var channels = http.GetGuildChannels(guild).Result
                .Where(c => c.Type == ChannelType.GuildText);
            var channel = channels.Single(c => c.Name.IsSome(n => n == "make_bot_go"));

            var command = new CommandInterface(http, channel.Id);
            var commands = new Commands(http);
            command.AddListeners(commands);

            Console.ReadKey();
        }

        private static void httpDemo() {
            var http = new HttpBotInterface();
            var guilds = http.GetAccessibleGuilds().Result;
            Console.WriteLine($"Accessible guilds: {guilds.Select(g => g.Name).ToSequenceString()}");

            var guild = guilds.Single(g => g.Name == "Anime_NSFW");
            var channels = http.GetGuildChannels(guild).Result
                .Where(c => c.Type == ChannelType.GuildText);
            Console.WriteLine($"{guild.Name} text channels: {channels.Select(c => c.Name).SelectSome().ToSequenceString()}");
            Console.WriteLine();

            var channel = channels.Single(c => c.Name.IsSome(n => n == "make_bot_go"));

            channel.LastMessageId.IfSome(latestMessageId => {
                var messagesBack = 5;
                var latestMessage = http.GetMessage(channel.Id, latestMessageId).Result;
                var latestMessages =
                    http.GetMessages(channel.Id, limit: messagesBack - 1, beforeMessage: latestMessageId).Result
                        .Append(latestMessage)
                        .OrderBy(m => m.Timestamp);

                Console.WriteLine($"Latest {messagesBack} messages to {channel.Name.Value}:");

                foreach (Message m in latestMessages) {
                    var str = $"{m.Author.Username} ({m.Timestamp})";
                    if (m.Type == MessageType.Default) {
                        if (m.Content != "")
                            str += $":\n\t{m.Content}";
                        foreach (Embed e in m.Embeds) {
                            str += $"\n\t{e.Title.Default("")} {e.Type.Map(t => ": " + t).Default("")}";
                            e.Fields.IfSome(fields => {
                                foreach (EmbedField f in fields)
                                    str += $"\n\t\t{f.Name} : {f.Value}";
                            });
                        }
                    } else
                        str += $" {Regex.Replace(m.Type.ToString(), "(?<=[^\\s])[A-Z]", " $0")}";
                    if (m.EditedTimestamp.IsSome())
                        str += $" (edited {m.EditedTimestamp.Value}";
                    str += "\n";

                    Console.WriteLine(str);
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Inverting reactions to latest message.");
                latestMessage.Reactions.IfSome(reactions => {
                    foreach (var reaction in reactions) {
                        if (reaction.Me) {
                            http.DeleteReaction(latestMessage, reaction.Emoji).Wait();
                        } else {
                            http.CreateReaction(latestMessage, reaction.Emoji).Wait();
                        }
                        Thread.Sleep(1000);
                    }
                });
                Console.WriteLine("Done");
            });

            Console.ReadKey();
        }
    }
}