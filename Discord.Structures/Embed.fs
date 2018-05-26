namespace Discord.Structures

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Mjolnir.Core
open System.Runtime.InteropServices


type Embed =
    {
        /// <summary>title of embed</summary>
    
        [<JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)>]
        Title: string option

        /// <summary>type of embed (always "rich" for webhook embeds)</summary>
        [<JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)>]
        Type: string option

        /// <summary>description of embed</summary>
        [<JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)>]
        Description: string option

        /// <summary>url of embed</summary>
        [<JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)>]
        Url: string option

        /// <summary>timestamp of embed content</summary>
        [<JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)>]
        Timestamp: DateTime option

        /// <summary>color code of the embed</summary>
        [<JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)>]
        Color: int option
        (*
        /// <summary>footer information</summary>
        [<JsonProperty("footer", NullValueHandling = NullValueHandling.Ignore)>]
        Footer: EmbedFooter option

        /// <summary>image information</summary>
        [<JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)>]
        Image: EmbedImage option

        /// <summary>thumbnail information</summary>
        [<JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)>]
        Thumbnail: EmbedThumbnail option

        /// <summary>video information</summary>
        [<JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)>]
        Video: EmbedVideo option

        /// <summary>provider information</summary>
        [<JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)>]
        Provider: EmbedProvider option

        /// <summary>author information</summary>
        [<JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)>]
        Author: EmbedAuthor option
        *)
        /// <summary>fields information</summary>
        [<JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)>]
        Fields: EmbedField array option

    }

    static member Deserialize str = JsonConvert.DeserializeObject<Embed>(str, General.serializationOpts)
    member this.Serialize () = JsonConvert.SerializeObject(this, General.serializationOpts)


    static member Build([<Optional;DefaultParameterValue("")>] title: string,
                        [<Optional;DefaultParameterValue("")>] typ: string ,
                        [<Optional;DefaultParameterValue("")>] description: string ,
                        [<Optional;DefaultParameterValue("")>] url: string ,
                        [<Optional;DefaultParameterValue(null:DateTime array)>] timestamp: DateTime array ,
                        [<Optional;DefaultParameterValue(-1)>] color: int ,
                        [<Optional;DefaultParameterValue(null: EmbedField array)>] fields: EmbedField array ) =
        {
            Title = if title = "" then None else Some(title)
            Type = if typ = "" then None else Some(typ)
            Description = if description = "" then None else Some(description)
            Url = if url = "" then None else Some(url)
            Timestamp = if timestamp = null then None else Some(timestamp.[0])
            Color = if color = -1 then None else Some(color)
            Fields = if fields = null then None else Some(fields)
        }