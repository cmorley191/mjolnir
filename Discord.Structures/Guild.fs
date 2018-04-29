namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core

type Guild =
    {
        /// <summary>guild id</summary>
        [<JsonProperty("id")>]
        Id: Snowflake

        /// <summary>guild name (2-100 characters)</summary>
        [<JsonProperty("name")>]
        Name: string

        /// <summary>icon hash</summary>
        [<JsonProperty("icon")>]
        Icon: string option

        /// <summary>splash hash</summary>
        [<JsonProperty("splash")>]
        Splash: string option

        /// <summary>whether or not the user is the owner of the guild</summary>
        [<JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)>]
        Owner: bool option

        /// <summary>id of owner</summary>
        [<JsonProperty("owner_id")>]
        OwnerId: Snowflake

        /// <summary>total permissions for the user in the guild (does not include channel overrides)</summary>
        [<JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)>]
        Permissions: int option

        /// <summary>voice region id for the guild</summary>
        [<JsonProperty("region")>]
        Region: string

        /// <summary>id of afk channel</summary>
        [<JsonProperty("afk_channel_id")>]
        AfkChannelId: Snowflake option

        /// <summary>afk timeout in seconds</summary>
        [<JsonProperty("afk_timeout")>]
        AfkTimeout: int

        /// <summary>is this guild embeddable (e.g. widget)</summary>
        [<JsonProperty("embed_enabled", NullValueHandling = NullValueHandling.Ignore)>]
        EmbedEnabled: bool option

        /// <summary>id of embedded channel</summary>
        [<JsonProperty("embed_channel_id", NullValueHandling = NullValueHandling.Ignore)>]
        EmbedChannelId: Snowflake option

        /// <summary>verification level required for the guild</summary>
        [<JsonProperty("verification_level")>]
        VerificationLevel: int

        /// <summary>default message notifications level</summary>
        [<JsonProperty("default_message_notifications")>]
        DefaultMessageNotifications: int

        /// <summary>explicit content filter level</summary>
        [<JsonProperty("explicit_content_filter")>]
        ExplicitContentFilter: int

        /// <summary>roles in the guild</summary>
        [<JsonProperty("roles")>]
        Roles: Role array
        (*
        /// <summary>custom guild emojis</summary>
        [<JsonProperty("emojis")>]
        Emojis: Emoji array
        *)
        /// <summary>enabled guild features</summary>
        [<JsonProperty("features")>]
        Features: string array

        /// <summary>required MFA level for the guild</summary>
        [<JsonProperty("mfa_level")>]
        MfaLevel: int

        /// <summary>application id of the guild creator if it is bot-created</summary>
        [<JsonProperty("application_id")>]
        ApplicationId: Snowflake option

        /// <summary>whether or not the server widget is enabled</summary>
        [<JsonProperty("widget_enabled", NullValueHandling = NullValueHandling.Ignore)>]
        WidgetEnabled: bool option

        /// <summary>the channel id for the server widget</summary>
        [<JsonProperty("widget_channel_id", NullValueHandling = NullValueHandling.Ignore)>]
        WidgetChannelId: Snowflake option

        /// <summary>the id of the channel to which system messages are sent</summary>
        [<JsonProperty("system_channel_id")>]
        SystemChannelId: Snowflake option

        /// <summary>when this guild was joined at</summary>
        [<JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)>]
        JoinedAt: DateTime option

        /// <summary>whether this is considered a large guild</summary>
        [<JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)>]
        Large: bool option

        /// <summary>is this guild unavailable</summary>
        [<JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)>]
        Unavailable: bool option

        /// <summary>total number of members in this guild</summary>
        [<JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)>]
        MemberCount: int option
        (*
        /// <summary>(without the guild_id key)</summary>
        [<JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)>]
        VoiceStates: VoiceState array option

        /// <summary>users in the guild</summary>
        [<JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)>]
        Members: GuildMember array option
        *)
        /// <summary>channels in the guild</summary>
        [<JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)>]
        Channels: Channel array option
        (*
        /// <summary>presences of the users in the guild</summary>
        [<JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)>]
        Presences: PresenceUpdate array option
        *)
    }

    static member Deserialize str = JsonConvert.DeserializeObject<Guild>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)