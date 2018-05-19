using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Payload {
    internal class VoiceReadyPayload {

        [JsonProperty(PropertyName = "ssrc")]
        public int SSRC { get; set; }

        [JsonProperty(PropertyName = "port")]
        public int UdpPort { get; set; }

        [JsonProperty(PropertyName = "modes")]
        public string[] SupportedEncryptionModes { get; set; }
    }
}
