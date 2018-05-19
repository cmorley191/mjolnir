using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord.Gateway.Models.Payload {
    internal class HelloPayload {
        /// <summary>
        ///     Gets or sets the heartbeat interval.
        /// </summary>
        /// <value>
        ///     The heartbeat interval.
        /// </value>
        [JsonProperty(PropertyName = "heartbeat_interval")]
        public int HeartbeatInterval { get; set; }

        /// <summary>
        ///     Gets or sets the trace.
        /// </summary>
        /// <value>
        ///     The trace.
        /// </value>
        [JsonProperty(PropertyName = "_trace")]
        public IEnumerable<string> Trace { get; set; }
    }
}