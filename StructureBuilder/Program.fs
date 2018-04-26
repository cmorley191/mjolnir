open System
open System.Text.RegularExpressions
open System.IO
open System.Linq

let capitalize (s:string) = s.[0].ToString().ToUpper() + s.Substring(1)

type Item =
  private {
    DiscordName: string
    DiscordType: string
    Description: string
    ArrayType: string option
    ObjectType: string option
    RealName: string
    RealType: string
  }

  static member Construct [fieldname:string; fieldtype:string; description:string] =
    let fieldname = Regex.Replace(fieldname, "\\s", " ")
    let fieldtype = Regex.Replace(fieldtype, "\\s", " ")
    let description = Regex.Replace(description, "\\s", " ")

    let torealname (n:string) =
      n.Split([|'_'; ' '|])
      |> Array.map capitalize
      |> String.concat ""

    let interpretPrimitive (p:string) =
      match p with
      | "integer" -> "int"
      | "snowflake" -> "Snowflake"
      | "ISO8601 timestamp" -> "DateTime"
      | _ -> p

    let (name, optional) = 
      let handleOptional (n:string, o) =
        if n.EndsWith "?" then
          ((n.Remove (n.Length - 1)), true)
        else (n, false)
      let handleAsterisk (n:string, o) =
        if n.EndsWith("*") then
          (n.Substring(0, n.Length - 1), o)
        else (n, o)
      let doTrim (n:string, o) =
        (n.Trim(), o)

      (fieldname, false)
      |> handleAsterisk
      |> doTrim
      |> handleOptional
      |> handleAsterisk
      |> doTrim

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
      let arraypattern = "^array\sof\s(partial\s|)(.*)\sobjects$"
      if Regex.IsMatch(``type``, arraypattern) then
        let match_ = Regex.Match(``type``, arraypattern)
        Some (match_.Groups.[2].ToString() |> torealname)
      else None

    let simplearraytype =
      let arraypattern = "^array\sof\s(.*)s$"
      if arraytype.IsNone && Regex.IsMatch(``type``, arraypattern) then
        let match_ = Regex.Match(``type``, arraypattern)
        Some (match_.Groups.[1].ToString() |> interpretPrimitive)
      else None

    let objecttype =
      let objectpattern = "^(.*)\sobject$"
      if arraytype.IsNone && Regex.IsMatch(``type``, objectpattern) then
        let match_ = Regex.Match(``type``, objectpattern)
        Some (match_.Groups.[1].ToString() |> torealname)
      else None

    {
      DiscordName = name
      DiscordType = fieldtype
      Description = description
      ArrayType = arraytype
      ObjectType = objecttype
      RealName = realname
      RealType =
        let handleNullable t = if nullable then t + " option" else t
        let handleOptional t = if optional then t + " option" else t
        let handlePrimitive t = interpretPrimitive t
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
          else if simplearraytype.IsSome then
            simplearraytype.Value + " array"
          else t

        ``type``
        |> handlePrimitive
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

  outputfn """type %s =
  {""" name

  for item in items do
    outputfn """    /// <summary>%s</summary>
    [<JsonProperty("%s")>]
    %s: %s
    """ item.Description item.DiscordName item.RealName item.RealType

  outputfn """  }

  static member Deserialize str = JsonConvert.DeserializeObject<%s>(str, General.serializationOpts)    
  member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)""" name

  main args

  0