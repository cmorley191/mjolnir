module rec Discord.Structures.Channel

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Discord.Structures.RawTypes
open Mjolnir.Core
open Discord.Structures.Overwrite
open Discord.Structures.User
type Channel =
  {
    /// <summary>the id of this channel</summary>
    [<JsonProperty("id")>]
    Id: Snowflake

    /// <summary>the type of channel</summary>
    [<JsonProperty("type")>]
    Type: int

    /// <summary>the id of the guild</summary>
    [<JsonProperty("guild_id")>]
    GuildId: Snowflake option

    /// <summary>sorting position of the channel</summary>
    [<JsonProperty("position")>]
    Position: int option

    /// <summary>explicit permission overwrites for members and roles</summary>
    [<JsonProperty("permission_overwrites")>]
    PermissionOverwrites: Overwrite array option

    /// <summary>the name of the channel (2-100 characters)</summary>
    [<JsonProperty("name")>]
    Name: string option

    /// <summary>the channel topic (0-1024 characters)</summary>
    [<JsonProperty("topic")>]
    Topic: string option option

    /// <summary>if the channel is nsfw</summary>
    [<JsonProperty("nsfw")>]
    Nsfw: bool option

    /// <summary>the id of the last message sent in this channel (may not point to an existing or valid message)</summary>
    [<JsonProperty("last_message_id")>]
    LastMessageId: Snowflake option option

    /// <summary>the bitrate (in bits) of the voice channel</summary>
    [<JsonProperty("bitrate")>]
    Bitrate: int option

    /// <summary>the user limit of the voice channel</summary>
    [<JsonProperty("user_limit")>]
    UserLimit: int option

    /// <summary>the recipients of the DM</summary>
    [<JsonProperty("recipients")>]
    Recipients: User array option

    /// <summary>icon hash</summary>
    [<JsonProperty("icon")>]
    Icon: string option option

    /// <summary>id of the DM creator</summary>
    [<JsonProperty("owner_id")>]
    OwnerId: Snowflake option

    /// <summary>application id of the group DM creator if it is bot-created</summary>
    [<JsonProperty("application_id")>]
    ApplicationId: Snowflake option

    /// <summary>id of the parent category for a channel</summary>
    [<JsonProperty("parent_id")>]
    ParentId: Snowflake option option

    /// <summary>when the last pinned message was pinned</summary>
    [<JsonProperty("last_pin_timestamp")>]
    LastPinTimestamp: DateTime option

  }

  static member Deserialize str = JsonConvert.DeserializeObject<Channel>(str, General.serializationOpts)
  member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)
