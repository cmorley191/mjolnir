using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models {
    enum OpCodeTypes {
        /// <summary>
        /// Dispatches an event (receive only).
        /// </summary>
        Dispatch = 0,

        /// <summary>
        /// The heartbeat Message Op Code, when we send our
        /// heart to the server
        /// </summary>
        HeartbeatMsg = 1,

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