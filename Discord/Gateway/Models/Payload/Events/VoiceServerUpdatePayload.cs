using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Payload.Events {
    internal class VoiceServerUpdatePayload {

        [JsonProperty(PropertyName = "token")]
        public string VoiceConnectionToken { get; set; }

        [JsonProperty(PropertyName = "guild_id")]
        public long GuildId { get; set; }

        /// <summary>
        /// The voice server host.
        /// </summary>
        [JsonProperty(PropertyName = "endpoint")]
        public string Endpoint { get; set; }

    }
}
