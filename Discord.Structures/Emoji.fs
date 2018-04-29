namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core

type Emoji =
    {
        /// <summary>emoji id</summary>
        [<JsonProperty("id")>]
        Id: Snowflake option

        /// <summary>emoji name</summary>
        [<JsonProperty("name")>]
        Name: string

        /// <summary>roles this emoji is whitelisted to</summary>
        [<JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)>]
        Roles: Snowflake array option

        /// <summary>user that created this emoji</summary>
        [<JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)>]
        User: User option

        /// <summary>whether this emoji must be wrapped in colons</summary>
        [<JsonProperty("require_colons", NullValueHandling = NullValueHandling.Ignore)>]
        RequireColons: bool option

        /// <summary>whether this emoji is managed</summary>
        [<JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)>]
        Managed: bool option

        /// <summary>whether this emoji is animated</summary>
        [<JsonProperty("animated", NullValueHandling = NullValueHandling.Ignore)>]
        Animated: bool option

    }

    static member Deserialize str = JsonConvert.DeserializeObject<Emoji>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)