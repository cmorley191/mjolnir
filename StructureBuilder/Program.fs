open System
open System.Text.RegularExpressions
open System.IO
open MjolnirCore

let capitalize (s:string) = s.[0].ToString().ToUpper() + s.Substring(1)

type Item =
  private {
    DiscordName: string
    DiscordType: string
    Description: string
    ArrayType: string option
    JsonName: string
    JsonType: string
    ToRealCode: string
    FromRealCode: string
    RealName: string
    RealType: string
  }

  static member Construct [fieldname:string; fieldtype:string; description:string] =
    let (name, optional) = 
      let handleOptional (n:string, o) =
        if n.EndsWith "?" then
          ((n.Remove (n.Length - 1)), true)
        else (n, false)
      let handleTypeName (n:string, o) =
        if n = "type" then
          ("``type``", o)
        else (n, o)

      (fieldname, true)
      |> handleOptional
      |> handleTypeName

    let (``type``, nullable) =
      if fieldtype.StartsWith("?") then 
        (fieldtype.Substring(1), true)
      else (fieldtype, false)


    let realname =
      if name = "``type``" then "Type" else
      name.Split([|'_'|])
      |> Array.map capitalize
      |> String.concat ""

    let arraytype = 
      let arraypattern = "array\sof\s([^\\s]*)\sobjects"
      if Regex.IsMatch(``type``, arraypattern) then
        let match_ = Regex.Match(``type``, arraypattern)
        Some (match_.Groups.[1].ToString() |> capitalize)
      else None

    {
      DiscordName = fieldname
      DiscordType = fieldtype
      Description = description
      ArrayType = arraytype
      JsonName = name

      JsonType = 
        let handleOptional t = if optional then t + " option" else t
        let handleSnowflake t = if t = "snowflake" then "string" else t
        let handleInteger t = if t = "integer" then "int" else t
        let handleDatetime t = if t = "ISO8601 timestamp" then "string" else t
        let handleArray (t:string) = if t.StartsWith("array of") then "JArray" else t

        ``type``
        |> handleSnowflake
        |> handleInteger
        |> handleDatetime
        |> handleArray
        |> handleOptional

      ToRealCode =
        let modifiers =
          let handleOptional m = 
            if optional then 
              m |> List.map (fun m -> "Option.map (" + m + ")")
            else m
          let handleSnowflake m = if ``type`` = "snowflake" then m @ ["snowflake"] else m
          let handleDatetime m =
            if ``type`` = "ISO8601 timestamp" then
              "fun s -> DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind)" :: m
            else m
          let handleArray m = 
            if arraytype.IsSome then
              (sprintf "fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> %s.Deserialize) |> Seq.toArray" arraytype.Value) :: m
            else m

          ([]:string list)
          |> handleArray
          |> handleSnowflake
          |> handleDatetime
          |> handleOptional
          |> Seq.map (fun m -> " |> " + m)
          |> String.concat ""

        sprintf "this.%s%s" name modifiers

      FromRealCode =
        let modifiers =
          let handleOptional m = 
            if optional then 
              m |> List.map (fun m -> "Option.map (" + m + ")")
            else m
          let handleSnowflake m = if ``type`` = "snowflake" then m @ ["fun x -> x.ToString()"] else m
          let handleDatetime m =
            if ``type`` = "ISO8601 timestamp" then
              "fun d -> d.ToString(\"s\", System.Globalization.CultureInfo.InvariantCulture)" :: m
            else m
          let handleArray m = 
            if arraytype.IsSome then
              "Seq.map (fun x -> x.Serialize) >> Seq.toArray >> JArray" :: m
            else m

          ([]:string list)
          |> handleArray
          |> handleSnowflake
          |> handleDatetime
          |> handleOptional
          |> Seq.map (fun m -> " |> " + m)
          |> String.concat ""

        sprintf "x.%s%s" realname modifiers

      RealName = realname
      RealType =
        let handleOptional t = if optional then t + " option" else t
        let handleSnowflake t = if t = "snowflake" then "Snowflake" else t
        let handleInteger t = if t = "integer" then "int" else t
        let handleDatetime t = if t = "ISO8601 timestamp" then "DateTime" else t
        let handleArray (t:string) = 
          if arraytype.IsSome then 
            arraytype.Value + " array"
          else t

        ``type``
        |> handleSnowflake
        |> handleInteger
        |> handleDatetime
        |> handleArray
        |> handleOptional
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
      |> Seq.map(fun s -> if s.StartsWith("\"") then s.Substring(1, s.Length - 2) else s)
      |> Seq.toList
    )

  let items = data |> Seq.map Item.Construct

  outputfn """module rec Discord.Structures.%s

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Discord.Structures.RawTypes""" name

  for item in items do
    if item.ArrayType.IsSome then
      outputfn "open Discord.Structures.%s" item.ArrayType.Value

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
    
  member this.Serialize =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = %sJson.FromReal this
    JsonConvert.SerializeObject(intermed, opts)""" name name

  Console.ReadLine() |> ignore

  0