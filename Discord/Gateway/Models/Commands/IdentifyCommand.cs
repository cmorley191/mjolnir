using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models.Commands {
    internal class IdentifyCommand {

        [JsonProperty(PropertyName = "op")]
        public const int OpCode = (int)MainOpCodeTypes.Identify;

        [JsonProperty(PropertyName = "d")]
        public IdentifyPayload Data { get; set; } = new IdentifyPayload();

        internal class IdentifyPayload {

            [JsonProperty(PropertyName = "token")]
            public string AuthenticationToken { get; set; }

            [JsonProperty(PropertyName = "properties")]
            public IdentifyConnectionProperties ConnectionProperties { get; set; } = new IdentifyConnectionProperties();

            internal class IdentifyConnectionProperties {

                [JsonProperty(PropertyName = "$os")]
                public string OperatingSystemName { get; set; }

                [JsonProperty(PropertyName = "$browser")]
                public string BrowserName { get; set; }

                [JsonProperty(PropertyName = "$device")]
                public string DeviceName { get; set; }
            }

            /// <summary>
            /// Defaults to false.
            /// </summary>
            [JsonProperty(PropertyName = "compress", NullValueHandling = NullValueHandling.Ignore)]
            public bool? AllowCompression { get; set; } = null;

            /// <summary>
            /// Value between 50 and 250, total number of members where the gateway will stop sending offline members in the guild member list.
            /// Defaults to 50.
            /// </summary>
            [JsonProperty(PropertyName = "large_threshold", NullValueHandling = NullValueHandling.Ignore)]
            public int? OfflineMemberFilterGuildSizeThreshold { get; set; } = null;

            // TODO: shards

            // TODO: presence

            [JsonProperty(PropertyName = "presence")]
            public IdentifyConnectionPresence ConnectionPresence = new IdentifyConnectionPresence();

            internal class IdentifyConnectionPresence {

                [JsonProperty(PropertyName = "game")]
                public IdentifyConnectionGame ConnectionGame = new IdentifyConnectionGame();

                internal class IdentifyConnectionGame {
                    [JsonProperty(PropertyName = "name")]
                    public string Name { get; set; }

                    [JsonProperty(PropertyName = "type")]
                    public int Type = 2;
                }


                [JsonProperty(PropertyName = "status")]
                public string Status { get; set; }

                [JsonProperty(PropertyName = "since")]
                public long TimeStarted = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;

                [JsonProperty(PropertyName = "afk")]
                public bool AFK = false;
            }
        }

    }
}

