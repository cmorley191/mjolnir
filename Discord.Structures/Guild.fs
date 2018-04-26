module rec Discord.Structures.Guild

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Discord.Structures.RawTypes
open Mjolnir.Core
open Discord.Structures.Role
(*
open Discord.Structures.Emoji
open Discord.Structures.Partial voiceState
open Discord.Structures.GuildMember
*)
open Discord.Structures.Channel
(*
open Discord.Structures.Partial presenceUpdate
*)
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

    /// <summary>whether or not the user is the owner of the guild</summary>
    [<JsonProperty("owner")>]
    Owner: bool option

    /// <summary>id of owner</summary>
    [<JsonProperty("owner_id")>]
    OwnerId: Snowflake

    /// <summary>total permissions for the user in the guild (does not include channel overrides)</summary>
    [<JsonProperty("permissions")>]
    Permissions: int option

    /// <summary>voice region id for the guild</summary>
    [<JsonProperty("region")>]
    Region: string

    /// <summary>id of afk channel</summary>
    [<JsonProperty("afk_channel_id")>]
    AfkChannelId: Snowflake option

    /// <summary>afk timeout in seconds</summary>
    [<JsonProperty("afk_timeout")>]
    AfkTimeout: int

    /// <summary>is this guild embeddable (e.g. widget)</summary>
    [<JsonProperty("embed_enabled")>]
    EmbedEnabled: bool option

    /// <summary>id of embedded channel</summary>
    [<JsonProperty("embed_channel_id")>]
    EmbedChannelId: Snowflake option

    /// <summary>verification level required for the guild</summary>
    [<JsonProperty("verification_level")>]
    VerificationLevel: int

    /// <summary>default message notifications level</summary>
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

    /// <summary>required MFA level for the guild</summary>
    [<JsonProperty("mfa_level")>]
    MfaLevel: int

    /// <summary>application id of the guild creator if it is bot-created</summary>
    [<JsonProperty("application_id")>]
    ApplicationId: Snowflake option

    /// <summary>whether or not the server widget is enabled</summary>
    [<JsonProperty("widget_enabled")>]
    WidgetEnabled: bool option

    /// <summary>the channel id for the server widget</summary>
    [<JsonProperty("widget_channel_id")>]
    WidgetChannelId: Snowflake option

    /// <summary>the id of the channel to which system messages are sent</summary>
    [<JsonProperty("system_channel_id")>]
    SystemChannelId: Snowflake option

    /// <summary>when this guild was joined at</summary>
    [<JsonProperty("joined_at")>]
    JoinedAt: DateTime option

    /// <summary>whether this is considered a large guild</summary>
    [<JsonProperty("large")>]
    Large: bool option

    /// <summary>is this guild unavailable</summary>
    [<JsonProperty("unavailable")>]
    Unavailable: bool option

    /// <summary>total number of members in this guild</summary>
    [<JsonProperty("member_count")>]
    MemberCount: int option
    (*
    /// <summary>(without the guild_id key)</summary>
    [<JsonProperty("voice_states")>]
    VoiceStates: Partial voiceState array option
    
    /// <summary>users in the guild</summary>
    [<JsonProperty("members")>]
    Members: GuildMember array option
    *)
    /// <summary>channels in the guild</summary>
    [<JsonProperty("channels")>]
    Channels: Channel array option
    (*
    /// <summary>presences of the users in the guild</summary>
    [<JsonProperty("presences")>]
    Presences: Partial presenceUpdate array option
    *)
  }

  static member Deserialize str = JsonConvert.DeserializeObject<Guild>(str, General.serializationOpts)
  member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)