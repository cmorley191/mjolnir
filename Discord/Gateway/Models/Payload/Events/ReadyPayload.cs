using Discord.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Payload.Events {
    internal class ReadyPayload : DataPayload {

        [JsonProperty(PropertyName = "v")]
        public int GatewayProtocolVersion { get; set; }

        [JsonProperty(PropertyName = "user")]
        public User CurrentUser { get; set; }

        [JsonProperty(PropertyName = "private_channels")]
        public Channel[] AssociatedDirectMessageChannels { get; set; }

        [JsonProperty(PropertyName = "guilds")]
        public PartialGuild[] AssociatedGuilds { get; set; }

        /// <summary>
        /// Used for resuming connections.
        /// </summary>
        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }

        /// <summary>
        /// Used for debugging - the guilds the user is in.
        /// </summary>
        [JsonProperty(PropertyName = "_trace")]
        public string[] Trace { get; set; }
    }
}
