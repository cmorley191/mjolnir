namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core

type User =
    {
        /// <summary>the user's id</summary>
        [<JsonProperty("id")>]
        Id: Snowflake

        /// <summary>the user's username; not unique across the platform</summary>
        [<JsonProperty("username")>]
        Username: string

        /// <summary>the user's 4-digit discord-tag</summary>
        [<JsonProperty("discriminator")>]
        Discriminator: string

        /// <summary>the user's?avatar hash</summary>
        [<JsonProperty("avatar")>]
        Avatar: string option

        /// <summary>whether the user belongs to an OAuth2 application</summary>
        [<JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)>]
        Bot: bool option

    }

    static member Deserialize str = JsonConvert.DeserializeObject<User>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)