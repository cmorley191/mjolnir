namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core

type VoiceState =
    {
        /// <summary>the guild id this voice state is for</summary>
        [<JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)>]
        GuildId: Snowflake option

        /// <summary>the channel id this user is connected to</summary>
        [<JsonProperty("channel_id")>]
        ChannelId: Snowflake option

        /// <summary>the user id this voice state is for</summary>
        [<JsonProperty("user_id")>]
        UserId: Snowflake

        /// <summary>the session id for this voice state</summary>
        [<JsonProperty("session_id")>]
        SessionId: string

        /// <summary>whether this user is deafened by the server</summary>
        [<JsonProperty("deaf")>]
        Deaf: bool

        /// <summary>whether this user is muted by the server</summary>
        [<JsonProperty("mute")>]
        Mute: bool

        /// <summary>whether this user is locally deafened</summary>
        [<JsonProperty("self_deaf")>]
        SelfDeaf: bool

        /// <summary>whether this user is locally muted</summary>
        [<JsonProperty("self_mute")>]
        SelfMute: bool

        /// <summary>whether this user is muted by the current user</summary>
        [<JsonProperty("suppress")>]
        Suppress: bool

    }

    static member Deserialize str = JsonConvert.DeserializeObject<VoiceState>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)