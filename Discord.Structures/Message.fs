module rec Discord.Structures.Message

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Discord.Structures.RawTypes
open Mjolnir.Core
open Discord.Structures.User
open Discord.Structures.User
//open Discord.Structures.Attachment
//open Discord.Structures.Embed
//open Discord.Structures.Reaction
//open Discord.Structures.MessageActivity
//open Discord.Structures.MessageApplication

type MessageJson =
  {
    id: string
    channel_id: string
    author: UserJson
    content: string
    timestamp: string
    edited_timestamp: string
    tts: bool
    mention_everyone: bool
    mentions: JArray
    mention_roles: JArray
    //attachments: JArray
    //embeds: JArray
    //reactions: JArray option
    nonce: string option
    pinned: bool
    webhook_id: string option
    ``type``: int
    //activity: MessageActivityJson option
    //application: MessageApplicationJson option
  }

  member this.ToReal () : Message =
    {
      Id = this.id |> snowflake
      ChannelId = this.channel_id |> snowflake
      Author = this.author |> (fun x -> x.ToReal())
      Content = this.content
      Timestamp = this.timestamp |> (fun s -> DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind))
      EditedTimestamp = this.edited_timestamp |> Option.ofNullType |> Option.map (fun s -> DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind))
      Tts = this.tts
      MentionEveryone = this.mention_everyone
      Mentions = this.mentions |> (fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> User.Deserialize) |> Seq.toArray)
      MentionRoles = this.mention_roles |> (fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> snowflake) |> Seq.toArray)
      //Attachments = this.attachments |> (fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> Attachment.Deserialize) |> Seq.toArray)
      //Embeds = this.embeds |> (fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> Embed.Deserialize) |> Seq.toArray)
      //Reactions = this.reactions |> Option.map (fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> Reaction.Deserialize) |> Seq.toArray)
      Nonce = this.nonce |> Option.map (Option.ofNullType) |> Option.map (Option.map (snowflake))
      Pinned = this.pinned
      WebhookId = this.webhook_id |> Option.map (snowflake)
      Type = this.``type``
      //Activity = this.activity |> Option.map ((fun x -> x.ToReal()))
      //Application = this.application |> Option.map ((fun x -> x.ToReal()))
    }

  static member FromReal (x : Message) : MessageJson =
    {
      id = x.Id |> (fun x -> x.ToString())
      channel_id = x.ChannelId |> (fun x -> x.ToString())
      author = x.Author |> UserJson.FromReal
      content = x.Content
      timestamp = x.Timestamp |> (fun d -> d.ToString("s", System.Globalization.CultureInfo.InvariantCulture))
      edited_timestamp = x.EditedTimestamp |> Option.map (fun d -> d.ToString("s", System.Globalization.CultureInfo.InvariantCulture)) |> Option.toNullType
      tts = x.Tts
      mention_everyone = x.MentionEveryone
      mentions = x.Mentions |> Seq.map (fun x -> x.Serialize) |> Seq.toArray |> JArray
      mention_roles = x.MentionRoles |> Seq.map (fun x -> x.ToString()) |> Seq.toArray |> JArray
      //attachments = x.Attachments |> Seq.map (fun x -> x.Serialize) |> Seq.toArray |> JArray
      //embeds = x.Embeds |> Seq.map (fun x -> x.Serialize) |> Seq.toArray |> JArray
      //reactions = x.Reactions |> Option.map (Seq.map (fun x -> x.Serialize)) |> Option.map (Seq.toArray) |> Option.map (JArray)
      nonce = x.Nonce |> Option.map (Option.map (fun x -> x.ToString())) |> Option.map (Option.toNullType)
      pinned = x.Pinned
      webhook_id = x.WebhookId |> Option.map (fun x -> x.ToString())
      ``type`` = x.Type
      //activity = x.Activity |> Option.map (MessageActivityJson.FromReal)
      //application = x.Application |> Option.map (MessageApplicationJson.FromReal)
    }

type Message =
  {
    /// <summary>id of the message</summary>
    Id: Snowflake
    /// <summary>id of the channel the message was sent in</summary>
    ChannelId: Snowflake
    /// <summary>the author of this message (not guaranteed to be a valid user, see below)</summary>
    Author: User
    /// <summary>contents of the message</summary>
    Content: string
    /// <summary>when this message was sent</summary>
    Timestamp: DateTime
    /// <summary>when this message was edited (or null if never)</summary>
    EditedTimestamp: DateTime option
    /// <summary>whether this was a TTS message</summary>
    Tts: bool
    /// <summary>whether this message mentions everyone</summary>
    MentionEveryone: bool
    /// <summary>users specifically mentioned in the message</summary>
    Mentions: User array
    /// <summary>roles specifically mentioned in this message</summary>
    MentionRoles: Snowflake array
    /// <summary>any attached files</summary>
    //Attachments: Attachment array
    /// <summary>any embedded content</summary>
    //Embeds: Embed array
    /// <summary>reactions to the message</summary>
    //Reactions: Reaction array option
    /// <summary>used for validating a message was sent</summary>
    Nonce: Snowflake option option
    /// <summary>whether this message is pinned</summary>
    Pinned: bool
    /// <summary>if the message is generated by a webhook, this is the webhook's id</summary>
    WebhookId: Snowflake option
    /// <summary>type of message</summary>
    Type: int
    /// <summary>sent with Rich Presence-related chat embeds</summary>
    //Activity: MessageActivity option
    /// <summary>sent with Rich Presence-related chat embeds</summary>
    //Application: MessageApplication option
  }

  static member Deserialize str =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = JsonConvert.DeserializeObject<MessageJson>(str, opts)
    intermed.ToReal ()

  member this.Serialize () =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = MessageJson.FromReal this
    JsonConvert.SerializeObject(intermed, opts)