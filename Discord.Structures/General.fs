module Discord.Structures.General

open Newtonsoft.Json
open Newtonsoft.Json.FSharp

let serializationOpts = Serialisation.extend (JsonSerializerSettings())