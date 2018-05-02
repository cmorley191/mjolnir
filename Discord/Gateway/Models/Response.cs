using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Discord.Gateway.Models.Payload;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discord.Gateway.Models {
    [JsonObject(MemberSerialization.OptIn)]
    internal class Response {
        /// <summary>
        ///     The lazy data payload
        /// </summary>
        private readonly Lazy<DataPayload> lazyDataPayload;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Response" /> class.
        /// </summary>
        public Response() => lazyDataPayload = new Lazy<DataPayload>(GetDataPayload);

        /// <summary>
        ///     Gets the data payload.
        /// </summary>
        /// <value>
        ///     The data payload.
        /// </value>
        public DataPayload DataPayload => lazyDataPayload.Value;

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
        public JObject RawDataPayload { get; set; }

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
        private static Dictionary<int, Type> OpTypes { get; } = new Dictionary<int, Type>();

        /// <summary>
        ///     Gets the data payload.
        /// </summary>
        /// <returns></returns>
        private DataPayload GetDataPayload() {
            Type t;
            if (OpTypes.ContainsKey(OpCode)) {
                t = OpTypes[OpCode];
            }
            else {
                var payloadTypes =
                    from type in Assembly.GetAssembly(typeof(PayloadAttribute)).GetTypes()
                    where type.IsDefined(typeof(PayloadAttribute), false)
                    where
                        ((PayloadAttribute) Attribute.GetCustomAttribute(type, typeof(PayloadAttribute))).OpCode ==
                        OpCode
                    select type;

                var types = payloadTypes.ToList();

                Debug.Assert(types.Count == 1);


                t = types.First();
            }

            return (DataPayload) RawDataPayload.ToObject(t);
        }
    }
}