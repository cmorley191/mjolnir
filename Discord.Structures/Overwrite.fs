namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core

type Overwrite =
    {
        /// <summary>role or user id</summary>
        [<JsonProperty("id")>]
        Id: Snowflake

        /// <summary>either "role" or "member"</summary>
        [<JsonProperty("type")>]
        Type: string

        /// <summary>permission bit set</summary>
        [<JsonProperty("allow")>]
        Allow: int

        /// <summary>permission bit set</summary>
        [<JsonProperty("deny")>]
        Deny: int

    }

    static member Deserialize str = JsonConvert.DeserializeObject<Overwrite>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)