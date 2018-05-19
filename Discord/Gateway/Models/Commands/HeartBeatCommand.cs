using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Discord.Gateway.Models.Commands {
    internal class HeartBeatCommand {

        [JsonProperty(PropertyName = "op")]
        public int OpCode { get; set; }

        [JsonProperty(PropertyName = "d", NullValueHandling = NullValueHandling.Include)]
        public int? Sequence { get; set; }
    }
}