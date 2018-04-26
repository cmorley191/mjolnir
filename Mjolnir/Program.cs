using System;
using System.IO;
using Discord;
using DotNetEnv;
using MjolnirCore;
using static Discord.Structures.Channel;
using static Discord.Structures.Message;

namespace Mjolnir {
    internal class Program {
        private static void Main(string[] args) {
            Env.Load(Path.Combine(EnvironmentHelper.SolutionFolderPath, ".env"));

            var gate = new GatewayClient();
            gate.Connect();

            Console.Write("Press Enter to Terminate: ");
            Console.ReadKey();

//            var http = new HttpBotInterface();
//            var channelId = "388253588624769034";
//            var channel = Channel.Deserialize(http.MakeRequest(resource: $"channels/{channelId}").Result);
//            var messageId = channel.LastMessageId.Value;
//            var message =
//                Message.Deserialize(http.MakeRequest(resource: $"channels/{channelId}/messages/{messageId}").Result);
//            Console.WriteLine($"Latest message to {channel.Name.Value}: \n" +
//                              $"{message.Author.Username} ({message.Timestamp}): {message.Content}\n" +
//                              (message.EditedTimestamp.IsSome()     
//                                  ? $"(Edited {message.EditedTimestamp.Value.ToString()})"
//                                  : ""));
//            Console.ReadKey();
        }
    }
}