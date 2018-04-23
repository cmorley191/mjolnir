using System.Diagnostics;
using System.IO;
using Discord;
using DotNetEnv;
using MjolnirCore;
using static Discord.Structures.Channel;
using static Discord.Structures.Message;

namespace Mjolnir
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Env.Load(Path.Combine(EnvironmentHelper.SolutionFolderPath, ".env"));

			var http = new HttpBotInterface();
			var channelId = "410221891236659201";
			var channel = Channel.Deserialize(http.MakeRequest(resource: $"channels/{channelId}").Result);
			var messageId = channel.LastMessageId.Value;
			var message = Message.Deserialize(http.MakeRequest(resource: $"channels/{channelId}/messages/{messageId}").Result);
			Debug.WriteLine($"Latest message to {channel.Name.Value}: \n" +
			                $"{message.Author.Username} ({message.Timestamp}): {message.Content}\n" +
			                (message.EditedTimestamp.IsSome() ? $"(Edited {message.EditedTimestamp.Value.ToString()})" : ""));
		}
	}
}