module rec Discord.Structures.Overwrite

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Discord.Structures.RawTypes

type OverwriteJson =
  {
    id: string
    ``type``: string
    allow: int
    deny: int
  }

  member this.ToReal () : Overwrite =
    {
      Id = this.id |> snowflake
      Type = this.``type``
      Allow = this.allow
      Deny = this.deny
    }

  static member FromReal (x : Overwrite) : OverwriteJson =
    {
      id = x.Id |> fun x -> x.ToString()
      ``type`` = x.Type
      allow = x.Allow
      deny = x.Deny
    }

type Overwrite =
  {
    /// <summary>role or user id</summary>
    Id: Snowflake
    /// <summary>either ""role"" or ""member""</summary>
    Type: string
    /// <summary>permission bit set</summary>
    Allow: int
    /// <summary>permission bit set</summary>
    Deny: int
  }

  static member Deserialize str =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = JsonConvert.DeserializeObject<OverwriteJson>(str, opts)
    intermed.ToReal ()

  member this.Serialize =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = OverwriteJson.FromReal this
    JsonConvert.SerializeObject(intermed, opts)