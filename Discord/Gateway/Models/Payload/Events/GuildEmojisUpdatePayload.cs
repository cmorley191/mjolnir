using Discord.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Payload.Events {
    public class GuildEmojisUpdatePayload {

        [JsonProperty(PropertyName = "guild_id")]
        public long GuildId { get; set; }

        [JsonProperty(PropertyName = "emojis")]
        public Emoji[] Emojis { get; set; }
    }
}
