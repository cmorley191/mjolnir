using System;
using System.IO;
using System.Linq;
using Discord;
using DotNetEnv;
using MjolnirCore;
using MjolnirCore.Extensions;
using static Discord.Structures.Channel;
using static Discord.Structures.Message;

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
            var channels = http.GetGuildChannels(guild).Result;
            Console.WriteLine($"{guild.Name} channels: {channels.Select(c => c.Name).SelectSome().ToSequenceString()}");

            var channel = channels.Single(c => c.Name.IsSome(n => n == "make_bot_go"));

            channel.LastMessageId.IfSome(messageId => {
                var lastMessage = http.GetMessage(channel, messageId.Value).Result;
                Console.WriteLine($"Latest message to {channel.Name.Value}: \n" +
                                  $"{lastMessage.Author.Username} ({lastMessage.Timestamp}): {lastMessage.Content}\n" +
                                  (lastMessage.EditedTimestamp.IsSome()
                                      ? $"(Edited {lastMessage.EditedTimestamp.Value.ToString()})"
                                      : ""));
            });
            Console.ReadKey();
        }
    }
}