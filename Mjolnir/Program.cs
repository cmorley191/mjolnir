using Discord;
using static Discord.Structures.Channel;
using static Discord.Structures.Message;
using MjolnirCore;
using MjolnirCore.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mjolnir {
  class Program {
    static void Main(string[] args) {
      DotNetEnv.Env.Load(Path.Combine(EnvironmentHelper.SolutionFolderPath, ".env"));

      HttpBotInterface http = new HttpBotInterface();
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
