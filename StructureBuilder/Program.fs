open System
open System.Text.RegularExpressions
open System.IO
open MjolnirCore
open System.Linq

let capitalize (s:string) = s.[0].ToString().ToUpper() + s.Substring(1)

type Item =
  private {
    DiscordName: string
    DiscordType: string
    Description: string
    ArrayType: string option
    ObjectType: string option
    JsonName: string
    JsonType: string
    ToRealCode: string
    FromRealCode: string
    RealName: string
    RealType: string
  }

  static member Construct [fieldname:string; fieldtype:string; description:string] =
    let torealname n =
      if n = "``type``" then "Type" else
      n.Split([|'_'; ' '|])
      |> Array.map capitalize
      |> String.concat ""

    let (name, optional) = 
      let handleOptional (n:string, o) =
        if n.EndsWith "?" then
          ((n.Remove (n.Length - 1)), true)
        else (n, false)
      let handleTypeName (n:string, o) =
        if n = "type" then
          ("``type``", o)
        else (n, o)
      let handleAsterisk (n:string, o) =
        if n.EndsWith("*") then
          (n.Substring(0, n.Length - 1), o)
        else (n, o)

      (fieldname, false)
      |> handleOptional
      |> handleAsterisk
      |> handleTypeName

    let (``type``, nullable) =
      if fieldtype.StartsWith("?") then 
        (fieldtype.Substring(1), true)
      else (fieldtype, false)

    let realname = torealname name

    let idarraytype =
      let arraypattern = "^array\sof\s(.*)\sobject\sids$"
      if Regex.IsMatch(``type``, arraypattern) then
        let match_ = Regex.Match(``type``, arraypattern)
        Some (match_.Groups.[1].ToString() |> torealname)
      else None

    let arraytype = 
      let arraypattern = "^array\sof\s(.*)\sobjects$"
      if Regex.IsMatch(``type``, arraypattern) then
        let match_ = Regex.Match(``type``, arraypattern)
        Some (match_.Groups.[1].ToString() |> torealname)
      else None

    let objecttype =
      let objectpattern = "^(.*)\sobject$"
      if arraytype.IsNone && Regex.IsMatch(``type``, objectpattern) then
        let match_ = Regex.Match(``type``, objectpattern)
        Some (match_.Groups.[1].ToString() |> torealname)
      else None

    {
      DiscordName = fieldname
      DiscordType = fieldtype
      Description = description
      ArrayType = arraytype
      ObjectType = objecttype
      JsonName = name

      JsonType = 
        let handleOptional t = if optional then t + " option" else t
        let handleSnowflake t = if t = "snowflake" then "string" else t
        let handleInteger t = if t = "integer" then "int" else t
        let handleDatetime t = if t = "ISO8601 timestamp" then "string" else t
        let handleIdArray t = if idarraytype.IsSome then "JArray" else t
        let handleObject t = if objecttype.IsSome then (sprintf "%sJson" objecttype.Value) else t
        let handleArray (t:string) = if arraytype.IsSome then "JArray" else t

        ``type``
        |> handleSnowflake
        |> handleInteger
        |> handleDatetime
        |> handleIdArray
        |> handleArray
        |> handleObject
        |> handleOptional

      ToRealCode =
        let modifiers =
          let handleNullable m =
            if nullable then
              "Option.ofNullType" :: (m |> List.map (fun m -> "Option.map (" + m + ")"))
            else m
          let handleOptional m = 
            if optional then 
              m |> List.map (fun m -> "Option.map (" + m + ")")
            else m
          let handleSnowflake m = if ``type`` = "snowflake" then m @ ["snowflake"] else m
          let handleDatetime m =
            if ``type`` = "ISO8601 timestamp" then
              "fun s -> DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind)" :: m
            else m
          let handleIdArray m =
            if idarraytype.IsSome then
              "fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> snowflake) |> Seq.toArray" :: m
            else m
          let handleObject m =
            if objecttype.IsSome then
              "(fun x -> x.ToReal())" :: m
            else m
          let handleArray m = 
            if arraytype.IsSome then
              (sprintf "fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> %s.Deserialize) |> Seq.toArray" arraytype.Value) :: m
            else m

          ([]:string list)
          |> handleArray
          |> handleObject
          |> handleSnowflake
          |> handleDatetime
          |> handleIdArray
          |> handleNullable
          |> handleOptional
          |> Seq.map (fun m -> " |> " + if m.StartsWith("fun") then "(" + m + ")" else m)
          |> String.concat ""

        sprintf "this.%s%s" name modifiers

      FromRealCode =
        let modifiers =
          let handleNullable m =
            if nullable then
              (m |> List.map (fun m -> "Option.map (" + m + ")")) @ ["Option.toNullType"]
            else m
          let handleOptional m = 
            if optional then 
              m |> List.map (fun m -> "Option.map (" + m + ")")
            else m
          let handleSnowflake m = if ``type`` = "snowflake" then m @ ["fun x -> x.ToString()"] else m
          let handleDatetime m =
            if ``type`` = "ISO8601 timestamp" then
              "fun d -> d.ToString(\"s\", System.Globalization.CultureInfo.InvariantCulture)" :: m
            else m
          let handleIdArray m =
            if idarraytype.IsSome then
              "Seq.map (fun x -> x.ToString())" :: "Seq.toArray" :: "JArray" :: m
            else m
          let handleObject m =
            if objecttype.IsSome then
              (sprintf "%sJson.FromReal" objecttype.Value) :: m
            else m
          let handleArray m = 
            if arraytype.IsSome then
              "Seq.map (fun x -> x.Serialize)" :: "Seq.toArray" :: "JArray" :: m
            else m

          ([]:string list)
          |> handleArray
          |> handleObject
          |> handleSnowflake
          |> handleDatetime
          |> handleIdArray
          |> handleNullable
          |> handleOptional
          |> Seq.map (fun m -> " |> " + if m.StartsWith("fun") then "(" + m + ")" else m)
          |> String.concat ""

        sprintf "x.%s%s" realname modifiers

      RealName = realname
      RealType =
        let handleNullable t = if nullable then t + " option" else t
        let handleOptional t = if optional then t + " option" else t
        let handleSnowflake t = if t = "snowflake" then "Snowflake" else t
        let handleInteger t = if t = "integer" then "int" else t
        let handleDatetime t = if t = "ISO8601 timestamp" then "DateTime" else t
        let handleIdArray t =
          if idarraytype.IsSome then 
            "Snowflake array"
          else t
        let handleObject t =
          if objecttype.IsSome then
            objecttype.Value
          else t
        let handleArray (t:string) = 
          if arraytype.IsSome then 
            arraytype.Value + " array"
          else t

        ``type``
        |> handleSnowflake
        |> handleInteger
        |> handleDatetime
        |> handleIdArray
        |> handleArray
        |> handleObject
        |> handleOptional
        |> handleNullable
    }

[<EntryPointAttribute>]
let rec main args =
  let outputfn = printfn

  let fname = 
    Console.WriteLine "Enter the structure name: "
    let input = Console.ReadLine()
    Path.Combine(EnvironmentHelper.SolutionFolderPath, "jsoncsvs", input + if input.EndsWith(".csv") then "" else ".csv")

  let name = fname.Substring(fname.LastIndexOf("\\") + 1, fname.Length - fname.LastIndexOf("\\") - 1 - ".csv".Length)
  let data =
    seq {
      use sr = new StreamReader (fname)
      while not sr.EndOfStream do
          yield sr.ReadLine ()
    }
    |> Seq.map (fun row ->
      row.Split(",")
      |> (fun parts -> [ parts.[0]; parts.[1]; parts |> Seq.skip 2 |> String.concat "," ])
      |> Seq.map (fun s -> if s.StartsWith("\"") && s.EndsWith("\"") then s.Substring(1, s.Length - 2) else s)
      |> Seq.toList
    )

  let items = data |> Seq.map Item.Construct

  outputfn """module rec Discord.Structures.%s

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Discord.Structures.RawTypes
open Mjolnir.Core""" name

  for item in items do
    if item.ArrayType.IsSome then
      outputfn "open Discord.Structures.%s" item.ArrayType.Value
    else if item.ObjectType.IsSome then
      outputfn "open Discord.Structures.%s" item.ObjectType.Value

  outputfn """
type %sJson =
  {""" name

  for item in items do
    outputfn "    %s: %s" item.JsonName item.JsonType

  outputfn """  }

  member this.ToReal () : %s =
    {""" name

  for item in items do
    outputfn "      %s = %s" item.RealName item.ToRealCode

  outputfn """    }

  static member FromReal (x : %s) : %sJson =
    {""" name name

  for item in items do
    outputfn "      %s = %s" item.JsonName item.FromRealCode

  outputfn """    }

type %s =
  {""" name

  for item in items do
    outputfn """    /// <summary>%s</summary>
    %s: %s""" item.Description item.RealName item.RealType

  outputfn """  }

  static member Deserialize str =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = JsonConvert.DeserializeObject<%sJson>(str, opts)
    intermed.ToReal ()
    
  member this.Serialize () =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = %sJson.FromReal this
    JsonConvert.SerializeObject(intermed, opts)""" name name

  Console.ReadLine() |> ignore

  0