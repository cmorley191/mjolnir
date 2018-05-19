using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Commands {
    internal class VoiceSelectProtocolCommand {

        [JsonProperty(PropertyName = "op")]
        public const int OpCode = (int)VoiceOpCodeTypes.SelectProtocol;

        [JsonProperty(PropertyName = "d")]
        public VoiceSelectProtocolPayload Data { get; set; } = new VoiceSelectProtocolPayload();

        internal class VoiceSelectProtocolPayload {

            [JsonProperty(PropertyName = "protocol")]
            public string Protocol { get; set; } = "udp";

            [JsonProperty(PropertyName = "data")]
            public VoiceProtocolData Data { get; set; } = new VoiceProtocolData();

            internal class VoiceProtocolData {

                [JsonProperty(PropertyName = "address")]
                public string ClientExternalIpAddress { get; set; }

                [JsonProperty(PropertyName = "port")]
                public int ClientPort { get; set; }

                [JsonProperty(PropertyName = "mode")]
                public string SelectedEncryptionMode { get; set; }
            }
        }
    }
}
