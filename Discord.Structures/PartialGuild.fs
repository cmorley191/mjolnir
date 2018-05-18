namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core

type PartialGuild =
    {
        /// <summary>guild id</summary>
        [<JsonProperty("id")>]
        Id: Snowflake

        /// <summary>is this guild unavailable</summary>
        [<JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)>]
        Unavailable: bool option
    }

    static member Deserialize str = JsonConvert.DeserializeObject<Guild>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)