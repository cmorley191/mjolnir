using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models {
    enum MainOpCodeTypes {
        /// <summary>
        /// Dispatches an event (receive only).
        /// </summary>
        Dispatch = 0,

        /// <summary>
        /// The heartbeat Message Op Code, when we send our
        /// heart to the server
        /// </summary>
        HeartbeatMsg = 1,

        Identify = 2,

        StatusUpdate = 3,

        VoiceStateUpdate = 4,

        VoiceServerPing = 5,

        Resume = 6,

        Reconnect = 7,

        RequestGuildMembers = 8,

        InvalidSession = 9,

        /// <summary>
        /// The hello op code
        /// </summary>
        Hello = 10,

        /// <summary>
        /// Op Code for a Heartbeat Acknowledge
        /// </summary>
        HeartbeatAck = 11
    }
}