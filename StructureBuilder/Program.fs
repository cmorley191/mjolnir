open System
open System.Text.RegularExpressions
open System.IO
open System.Linq

let capitalize (s:string) = s.[0].ToString().ToUpper() + s.Substring(1)

let (|RegexMatch|_|) patt s =
    if Regex.IsMatch(s, patt) then
        Some (Regex.Match(s, patt).Groups |> Seq.map string |> Seq.toArray)
    else None

type StructureFieldName =
    | Name of string
    | Optional of StructureFieldName

    static member parseDiscordName (n:string) =
        match n.Trim(' ', '*') with
        | RegexMatch "^(.*)\?$" groups -> groups.[1] |> StructureFieldName.parseDiscordName |> Optional
        | somethingElse -> somethingElse |> Name

    member this.DiscordString =
        match this with
        | Name s -> s
        | Optional s -> s.DiscordString

    member this.FSharpString = 
        match this with
        | Name s ->
            s.Split([|'_'; ' '|])
            |> Array.map capitalize
            |> String.concat ""
        | Optional n -> n.FSharpString

    member this.JsonProperties = [ sprintf "\"%s\"" this.DiscordString ]

type StructureFieldType =
    | Optional of StructureFieldType
    | Nullable of StructureFieldType
    | Integer | Timestamp | Snowflake 
    | OtherPrimitive of string
    | Structure of string
    | Array of StructureFieldType

    static member parseDiscordType name (t:string) =
        match name with
        | StructureFieldName.Optional n ->
            t |> StructureFieldType.parseDiscordType n |> Optional
        | Name _ ->
            match t.Trim(' ', '*') with
            | RegexMatch "^\?(.*)$" groups -> groups.[1] |> StructureFieldType.parseDiscordType name |> Nullable
            | "integer" -> Integer
            | "ISO8601 timestamp" -> Timestamp
            | "snowflake" -> Snowflake
            | RegexMatch "^array of (.*)s$" groups -> groups.[1] |> StructureFieldType.parseDiscordType name |> Array
            | RegexMatch "^(.*) object id$" _ -> Snowflake
            | RegexMatch "^(partial |)(.*) object$" groups -> (groups.[2] |> StructureFieldName.parseDiscordName).FSharpString |> Structure
            | somethingElse -> somethingElse |> OtherPrimitive

    member this.FSharpString =
        match this with
        | Optional (Nullable t) | Nullable (Optional t) 
            | Optional t | Nullable t -> t.FSharpString + " option"
        | Integer -> "int"
        | Timestamp -> "DateTime"
        | Snowflake -> "Snowflake"
        | OtherPrimitive t -> t
        | Structure t -> t
        | Array t -> t.FSharpString + " array"

    member this.JsonProperties =
        match this with
        | Optional t -> List.append t.JsonProperties ["NullValueHandling = NullValueHandling.Ignore"]
        | _ -> []

type StructureField =
    private {
        Name: StructureFieldName
        Type: StructureFieldType
        Properties: string list
        Description: string
    }

    static member Construct [discordname:string; discordtype:string; description:string] =
        let name = discordname |> StructureFieldName.parseDiscordName
        let ``type`` = discordtype |> StructureFieldType.parseDiscordType name
        {
            Name = name
            Type = ``type``
            Properties = List.append name.JsonProperties ``type``.JsonProperties
            Description = description
        }

[<EntryPointAttribute>]
let rec main args =
    let outputfn = printfn

    let fname = 
        Console.WriteLine "Enter the structure name: "
        let input = Console.ReadLine()
        if input = "exit" || input = "quit" then
            exit 0
        else
            Path.Combine("..", "..", "..", "..", "jsoncsvs", input + if input.EndsWith(".csv") then "" else ".csv")

    let name = fname.Substring(fname.LastIndexOf("\\") + 1, fname.Length - fname.LastIndexOf("\\") - 1 - ".csv".Length)

    let data =
        seq {
            use sr = new StreamReader (fname)
            while not sr.EndOfStream do
                    yield sr.ReadLine ()
        }
        |> Seq.map (fun row ->
            row.Split(",")
            |> Array.map (fun x -> Regex.Replace(x, "\\s", " "))
            |> Array.map (fun x -> Regex.Replace(x, "\"\"", "\""))
            |> (fun parts -> [ parts.[0]; parts.[1]; parts |> Seq.skip 2 |> String.concat "," ])
            |> Seq.map (fun s -> if s.StartsWith("\"") && s.EndsWith("\"") then s.Substring(1, s.Length - 2) else s)
            |> Seq.toList
        )

    let structureFields = data |> Seq.map StructureField.Construct

    outputfn """namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core

type %s =
    {""" name

    for item in structureFields do
        outputfn """        /// <summary>%s</summary>
        [<JsonProperty(%s)>]
        %s: %s
        """ item.Description (String.Join(", ", item.Properties)) item.Name.FSharpString item.Type.FSharpString

    outputfn """    }

    static member Deserialize str = JsonConvert.DeserializeObject<%s>(str, General.serializationOpts)        
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)""" name


    main args |> ignore
    0