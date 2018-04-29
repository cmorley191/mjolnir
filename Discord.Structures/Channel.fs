namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core

type Channel =
    {
        /// <summary>the id of this channel</summary>
        [<JsonProperty("id")>]
        Id: Snowflake

        /// <summary>the type of channel</summary>
        [<JsonProperty("type")>]
        Type: int

        /// <summary>the id of the guild</summary>
        [<JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)>]
        GuildId: Snowflake option

        /// <summary>sorting position of the channel</summary>
        [<JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)>]
        Position: int option

        /// <summary>explicit permission overwrites for members and roles</summary>
        [<JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)>]
        PermissionOverwrites: Overwrite array option

        /// <summary>the name of the channel (2-100 characters)</summary>
        [<JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)>]
        Name: string option

        /// <summary>the channel topic (0-1024 characters)</summary>
        [<JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)>]
        Topic: string option

        /// <summary>if the channel is nsfw</summary>
        [<JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)>]
        Nsfw: bool option

        /// <summary>the id of the last message sent in this channel (may not point to an existing or valid message)</summary>
        [<JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)>]
        LastMessageId: Snowflake option

        /// <summary>the bitrate (in bits) of the voice channel</summary>
        [<JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)>]
        Bitrate: int option

        /// <summary>the user limit of the voice channel</summary>
        [<JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)>]
        UserLimit: int option

        /// <summary>the recipients of the DM</summary>
        [<JsonProperty("recipients", NullValueHandling = NullValueHandling.Ignore)>]
        Recipients: User array option

        /// <summary>icon hash</summary>
        [<JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)>]
        Icon: string option

        /// <summary>id of the DM creator</summary>
        [<JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)>]
        OwnerId: Snowflake option

        /// <summary>application id of the group DM creator if it is bot-created</summary>
        [<JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)>]
        ApplicationId: Snowflake option

        /// <summary>id of the parent category for a channel</summary>
        [<JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)>]
        ParentId: Snowflake option

        /// <summary>when the last pinned message was pinned</summary>
        [<JsonProperty("last_pin_timestamp", NullValueHandling = NullValueHandling.Ignore)>]
        LastPinTimestamp: DateTime option

    }

    static member Deserialize str = JsonConvert.DeserializeObject<Channel>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)