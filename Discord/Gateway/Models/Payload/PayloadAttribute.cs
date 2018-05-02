using System;

namespace Discord.Gateway.Models.Payload {
    [AttributeUsage(AttributeTargets.Class)]
    internal class PayloadAttribute : Attribute {
        /// <summary>
        ///     Gets or sets the op code.
        /// </summary>
        /// <value>
        ///     The op code.
        /// </value>
        public int OpCode { get; set; }
    }
}