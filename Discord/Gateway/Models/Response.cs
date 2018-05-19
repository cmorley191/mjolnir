using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Discord.Gateway.Models.Payload;
using Discord.Gateway.Models.Payload.Events;
using Discord.Structures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discord.Gateway.Models {
    [JsonObject(MemberSerialization.OptIn)]
    internal class Response {

        [JsonProperty(PropertyName = "op")]
        public int OpCode { get; set; }

        [JsonProperty(PropertyName = "d")]
        public JToken RawDataPayload { get; set; }

        [JsonProperty(PropertyName = "s")]
        public int? SequenceNumber { get; set; }

        [JsonProperty(PropertyName = "t")]
        public string EventName { get; set; }

        internal static Dictionary<int, Type> OpTypes { get; } = new Dictionary<int, Type>();
        internal static Dictionary<string, Type> EventTypes { get; } = new Dictionary<string, Type>();

        public dynamic DeserializeDataPayload(Type targetType) => RawDataPayload.ToObject(targetType);
        public T DeserializeDataPayload<T>() => DeserializeDataPayload(typeof(T));
    }
}