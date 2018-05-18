using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Payload.Events {
    public class GuildIntegrationsUpdatePayload {

        [JsonProperty(PropertyName = "guild_id")]
        public long GuildId { get; set; }
    }
}
