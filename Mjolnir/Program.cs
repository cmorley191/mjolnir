using Discord;
using static Discord.Structures.Channel;
using MjolnirCore;
using MjolnirCore.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Mjolnir {
  class Program {
    static void Main(string[] args) {
      DotNetEnv.Env.Load(Path.Combine(EnvironmentHelper.SolutionFolderPath, ".env"));

      HttpBotInterface http = new HttpBotInterface();
      var channelId = "388253588624769034";
      var output = http.MakeRequest(resource: $"channels/{channelId}").Result;
      Debug.WriteLine(Channel.Deserialize(output).ToString());
    }
  }
}
