namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core
open System.Runtime.InteropServices

type Outbound_Message =
    {
        /// <summary>the message contents (up to 2000 characters),TRUE</summary>
        [<JsonProperty("content")>]
        Content: string

        /// <summary>a nonce that can be used for optimistic message sending,FALSE</summary>
        [<JsonProperty("nonce")>]
        Nonce: Snowflake option

        /// <summary>true if this is a TTS message,FALSE</summary>
        [<JsonProperty("tts")>]
        Tts: bool option
         (*
        /// <summary>the contents of the file being sent,"one of content, file, embeds (multipart/form-data only)"</summary>
        [<JsonProperty("file")>]
        File: file_contents
        *)
        /// <summary>embedded rich content,FALSE</summary>
        [<JsonProperty("embed")>]
        Embed: Embed option

        /// <summary>url-encoded JSON body used in place of the embed field,multipart/form-data only</summary>
        [<JsonProperty("payload_json")>]
        PayloadJson: string option

    }

    static member Deserialize str = JsonConvert.DeserializeObject<Outbound_Message>(str, General.serializationOpts)     
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)
    
    static member Build([<Optional;DefaultParameterValue("")>]content: string,
                        [<Optional;DefaultParameterValue(0L)>] nonce: Snowflake ,
                        [<Optional;DefaultParameterValue(false)>] tts: bool ,
                        [<Optional>] embed: Embed,
                        [<Optional;DefaultParameterValue("")>] payload: string ) =
        {
            Content = content
            Nonce = if nonce = 0L  then None else Some(nonce)
            Tts = if tts then Some(tts) else None
            Embed = if box embed = null then None else Some(embed)
            PayloadJson = if payload = "" then None else Some(payload)
        }