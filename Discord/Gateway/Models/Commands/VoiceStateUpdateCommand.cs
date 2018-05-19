using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Commands {
    internal class VoiceStateUpdateCommand {

        [JsonProperty(PropertyName = "op")]
        public const int OpCode = (int)MainOpCodeTypes.VoiceStateUpdate;

        [JsonProperty(PropertyName = "d")]
        public VoiceStateUpdatePayload Data { get; set; } = new VoiceStateUpdatePayload();

        internal class VoiceStateUpdatePayload {

            [JsonProperty(PropertyName = "guild_id")]
            public long GuildId { get; set; }

            /// <summary>
            /// Id of the voice channel client wants to join (or null if disconnecting).
            /// </summary>
            [JsonProperty(PropertyName = "channel_id")]
            public long? ChannelId { get; set; }

            [JsonProperty(PropertyName = "self_mute")]
            public bool MuteSelf { get; set; }

            [JsonProperty(PropertyName = "self_deaf")]
            public bool DeafenSelf { get; set; }
        }
    }
}
