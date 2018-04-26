module rec Discord.Structures.Role

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Discord.Structures.RawTypes
open Mjolnir.Core
type Role =
  {
    /// <summary>role id</summary>
    [<JsonProperty("id")>]
    Id: Snowflake

    /// <summary>role name</summary>
    [<JsonProperty("name")>]
    Name: string

    /// <summary>integer representation of hexadecimal color code</summary>
    [<JsonProperty("color")>]
    Color: int

    /// <summary>if this role is pinned in the user listing</summary>
    [<JsonProperty("hoist")>]
    Hoist: bool

    /// <summary>position of this role</summary>
    [<JsonProperty("position")>]
    Position: int

    /// <summary>permission bit set</summary>
    [<JsonProperty("permissions")>]
    Permissions: int

    /// <summary>whether this role is managed by an integration</summary>
    [<JsonProperty("managed")>]
    Managed: bool

  }

  static member Deserialize str = JsonConvert.DeserializeObject<Role>(str, General.serializationOpts)
  member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)