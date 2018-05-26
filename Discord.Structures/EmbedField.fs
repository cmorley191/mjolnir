namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core
open System.Runtime.InteropServices

type EmbedField =
    {
        /// <summary>name of the field</summary>
        [<JsonProperty("name")>]
        Name: string

        /// <summary>value of the field</summary>
        [<JsonProperty("value")>]
        Value: string

        /// <summary>whether or not this field should display inline</summary>
        [<JsonProperty("inline", NullValueHandling = NullValueHandling.Ignore)>]
        Inline: bool option

    }

    static member Deserialize str = JsonConvert.DeserializeObject<EmbedField>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)

    static member Build(name: string, value: string ,
                        [<Optional;DefaultParameterValue(false)>] inlin: bool) =
        {
            Name = name
            Value = value
            Inline = if inlin then None else Some(inlin)
        }