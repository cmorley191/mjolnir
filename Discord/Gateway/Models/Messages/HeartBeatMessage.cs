using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Discord.Gateway.Models.Messages {
    internal class HeartBeatMessage {
        /// <summary>
        /// The op code
        /// </summary>
        [JsonProperty(PropertyName = "op")] public const int OpCode = 1;

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        /// <value>
        /// The sequence.
        /// </value>
        [JsonProperty(PropertyName = "d")]
        public int? Sequence { get; set; }
    }
}