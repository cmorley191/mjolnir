namespace Discord.Structures

type Snowflake = int64

module General =

    open Newtonsoft.Json
    open Newtonsoft.Json.FSharp

    let serializationOpts = Serialisation.extend (JsonSerializerSettings())

    [<CompiledName("DeserializeMany")>]
    let deserializeMany (str:string) = JsonConvert.DeserializeObject<'a array>(str, serializationOpts)