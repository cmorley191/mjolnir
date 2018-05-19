using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Payload {
    internal class VoiceSessionDescriptionPayload {

        [JsonProperty(PropertyName = "mode")]
        public string EncryptionMode { get; set; }

        [JsonProperty(PropertyName = "secret_key")]
        public byte[] EncryptionKey { get; set; }
    }
}
