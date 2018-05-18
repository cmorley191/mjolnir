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
        /// <summary>
        ///     The lazy data payload
        /// </summary>
        private readonly Lazy<object> lazyDataPayload;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Response" /> class.
        /// </summary>
        public Response() => lazyDataPayload = new Lazy<object>(GetDataPayload);

        /// <summary>
        ///     Gets the data payload.
        /// </summary>
        /// <value>
        ///     The data payload.
        /// </value>
        public object DataPayload => lazyDataPayload.Value;

        /// <summary>
        ///     Gets or sets the op code.
        /// </summary>
        /// <value>
        ///     The op code.
        /// </value>
        [JsonProperty(PropertyName = "op")]
        public int OpCode { get; set; }

        /// <summary>
        ///     Gets or sets the raw data payload.
        /// </summary>
        /// <value>
        ///     The raw data payload.
        /// </value>
        [JsonProperty(PropertyName = "d")]
        public JContainer RawDataPayload { get; set; }

        /// <summary>
        ///     Gets or sets the sequence number.
        /// </summary>
        /// <value>
        ///     The sequence number.
        /// </value>
        [JsonProperty(PropertyName = "s")]
        public int? SequenceNumber { get; set; }

        /// <summary>
        ///     Gets or sets the name of the event.
        /// </summary>
        /// <value>
        ///     The name of the event.
        /// </value>
        [JsonProperty(PropertyName = "t")]
        public string EventName { get; set; }

        /// <summary>
        ///     Gets the op types.
        /// </summary>
        /// <value>
        ///     The op types.
        /// </value>
        internal static Dictionary<int, Type> OpTypes { get; } = new Dictionary<int, Type>();
        internal static Dictionary<string, Type> EventTypes { get; } = new Dictionary<string, Type>();

        /// <summary>
        ///     Gets the data payload.
        /// </summary>
        /// <returns></returns>
        private object GetDataPayload() {
            Type t;
            if (OpCode == (int)OpCodeTypes.Dispatch) {
                if (EventTypes.ContainsKey(EventName)) {
                    t = EventTypes[EventName];
                } else {
                    var supportedEventTypes =
                        typeof(EventType).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                        .Select(fi => (EventType)fi.GetValue(null));

                    t = supportedEventTypes.First(ev => ev.name == this.EventName).payloadType;
                    EventTypes[EventName] = t;
                }
            } else if (OpTypes.ContainsKey(OpCode)) {
                t = OpTypes[OpCode];
            } else {
                var typesOfAttribute =
                    from type in Assembly.GetAssembly(typeof(PayloadAttribute)).GetTypes()
                    where type.IsDefined(typeof(PayloadAttribute), false)
                    where ((PayloadAttribute)Attribute.GetCustomAttribute(type, typeof(PayloadAttribute))).OpCode == this.OpCode
                    select type;

                Debug.Assert(typesOfAttribute.Count() == 1);

                t = typesOfAttribute.First();
                OpTypes[OpCode] = t;
            }
            return RawDataPayload.ToObject(t);
        }
    }
}