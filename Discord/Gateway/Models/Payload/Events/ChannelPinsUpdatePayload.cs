using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Payload.Events {
    public class ChannelPinsUpdatePayload {

        [JsonProperty(PropertyName = "channel_id")]
        public long ChannelId { get; set; }

        [JsonProperty(PropertyName = "last_pin_timestamp")]
        public DateTime? LatestPinTimestamp { get; set; }
    }
}
