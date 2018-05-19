using Discord.Gateway.Models.Payload.Events;
using Discord.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Gateway.Models {
    public sealed class EventType {

        public readonly string Name;
        public readonly Type PayloadType;
        private EventType(string name, Type payloadType) {
            Name = name;
            PayloadType = payloadType;
        }

        internal static readonly EventType Ready = new EventType("READY", typeof(ReadyPayload));

        /// <summary>
        /// Sent when a new channel is created, relevant to the current user.
        /// </summary>
        public static readonly EventType ChannelCreate = new EventType("CHANNEL_CREATE", typeof(Channel));
        /// <summary>
        /// Sent when a channel is updated.
        /// </summary>
        public static readonly EventType ChannelUpdate = new EventType("CHANNEL_UPDATE", typeof(Channel));
        /// <summary>
        /// Sent when a channel relevant to the current user is deleted.
        /// </summary>
        public static readonly EventType ChannelDelete = new EventType("CHANNEL_DELETE", typeof(Channel));
        /// <summary>
        /// Sent when a message is pinned or unpinned in a text channel. This is not sent when a pinned message is deleted.
        /// </summary>
        public static readonly EventType ChannelPinsUpdate = new EventType("CHANNEL_PINS_UPDATE", typeof(ChannelPinsUpdatePayload));

        /// <summary>
        /// This event can be sent in three different scenarios:
        /// 1. When a user is initially connecting, to lazily load and backfill information for all unavailable guilds sent in the Ready event.
        /// 2. When a Guild becomes available again to the client.
        /// 3. When the current user joins a new Guild.
        /// </summary>
        public static readonly EventType GuildCreate = new EventType("GUILD_CREATE", typeof(Guild));
        /// <summary>
        /// Sent when a guild is updated.
        /// </summary>
        public static readonly EventType GuildUpdate = new EventType("GUILD_UPDATE", typeof(Guild));
        /// <summary>
        /// Sent when a guild becomes unavailable during a guild outage, or when the user leaves or is removed from a guild.
        /// If the unavailable field is not set, the user was removed from the guild.
        /// </summary>
        public static readonly EventType GuildDelete = new EventType("GUILD_DELETE", typeof(PartialGuild));

        /// <summary>
        /// Sent when a guild's emojis have been updated.
        /// </summary>
        public static readonly EventType GuildEmojisUpdate = new EventType("GUILD_EMOJIS_UPDATE", typeof(GuildEmojisUpdatePayload));
        /// <summary>
        /// Sent when a guild integration is updated.
        /// </summary>
        public static readonly EventType GuildIntegrationsUpdate = new EventType("GUILD_INTEGRATIONS_UPDATE", typeof(GuildIntegrationsUpdatePayload));

        /// <summary>
        /// Sent when a message is created.
        /// </summary>
        public static readonly EventType MessageCreate = new EventType("MESSAGE_CREATE", typeof(Message));

        // TODO: Message objects have loads of required fields, so this event will certainly break. We're gonna have to do some thinking on this one.
        /// <summary>
        /// Sent when a message is updated.
        /// Unlike creates, message updates may contain only a subset of the full message object payload (but will always contain an id and channel_id).
        /// </summary>
        public static readonly EventType MessageUpdate = new EventType("MESSAGE_UPDATE", typeof(Message));

        /// <summary>
        /// Sent when someone joins/leaves/moves voice channels.
        /// </summary>
        public static readonly EventType VoiceStateUpdate = new EventType("VOICE_STATE_UPDATE", typeof(VoiceState));
        /// <summary>
        /// Sent when a guild's voice server is updated. This is sent when initially connecting to voice, and when the current voice instance fails over to a new server.
        /// </summary>
        public static readonly EventType VoiceServerUpdate = new EventType("VOICE_SERVER_UPDATE", typeof(VoiceServerUpdatePayload));


        // TODO: copy in the rest from https://discordapp.com/developers/docs/topics/gateway#commands-and-events
        // Lots of them have major issues (suddenly any field in any structure is optional) that we're gonna have to handle
    }
}
