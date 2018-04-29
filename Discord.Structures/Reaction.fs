namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core

type Reaction =
    {
        /// <summary>times this emoji has been used to react</summary>
        [<JsonProperty("count")>]
        Count: int

        /// <summary>whether the current user reacted using this emoji</summary>
        [<JsonProperty("me")>]
        Me: bool

        /// <summary>emoji information</summary>
        [<JsonProperty("emoji")>]
        Emoji: Emoji

    }

    static member Deserialize str = JsonConvert.DeserializeObject<Reaction>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)