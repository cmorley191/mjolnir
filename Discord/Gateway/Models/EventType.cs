using Discord.Gateway.Models.Payload.Events;
using Discord.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models {
    public sealed class EventType {

        public readonly string name;
        public readonly Type payloadType;
        private EventType(string name, Type payloadType) {
            this.name = name;
            this.payloadType = payloadType;
        }

        public static readonly EventType Ready = new EventType("READY", typeof(ReadyPayload));

        public static readonly EventType GuildCreate = new EventType("GUILD_CREATE", typeof(Guild));
        public static readonly EventType ChannelCreate = new EventType("CHANNEL_CREATE", typeof(Channel));
        public static readonly EventType MessageCreate = new EventType("MESSAGE_CREATE", typeof(Message));
        // TODO: just copy in the rest from https://discordapp.com/developers/docs/topics/gateway#commands-and-events
    }
}
