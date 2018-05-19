using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Commands {
    internal class VoiceIdentifyCommand {

        [JsonProperty(PropertyName = "op")]
        public const int OpCode = (int)VoiceOpCodeTypes.Identify;

        [JsonProperty(PropertyName = "d")]
        public VoiceIdentifyPayload Data { get; set; } = new VoiceIdentifyPayload();

        internal class VoiceIdentifyPayload {

            [JsonProperty(PropertyName = "server_id")]
            public string ServerId { get; set; }

            [JsonProperty(PropertyName = "user_id")]
            public string UserId { get; set; }

            [JsonProperty(PropertyName = "session_id")]
            public string SessionId { get; set; }

            [JsonProperty(PropertyName = "token")]
            public string VoiceConnectionToken { get; set; }
        }
    }
}
