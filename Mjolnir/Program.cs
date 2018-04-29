﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Discord;
using Discord.Http;
using Discord.Structures;
using DotNetEnv;
using MjolnirCore;
using MjolnirCore.Extensions;

namespace Mjolnir {
    internal class Program {
        private static void Main(string[] args) {
            Env.Load(Path.Combine(EnvironmentHelper.SolutionFolderPath, ".env"));

            httpDemo();
        }

        private static void gatewayDemo() {
            var gate = new GatewayClient();
            gate.Connect();

            Console.Write("Press Enter to Terminate: ");
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
                        var emojiId = (reaction.Emoji.Id.IsSome()) ? reaction.Emoji.Id.Value.ToString() : reaction.Emoji.Name;
                        if (reaction.Me) {
                            http.DeleteReaction(latestMessage.ChannelId, latestMessage.Id, emojiId).Wait();
                        } else {
                            http.CreateReaction(latestMessage.ChannelId, latestMessage.Id, emojiId).Wait();
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