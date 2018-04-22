module rec Discord.Structures.Channel

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Discord.Structures.RawTypes
open Discord.Structures.Overwrite
open Discord.Structures.User

type ChannelJson =
  {
    id: string
    ``type``: int
    guild_id: string option
    position: int option
    permission_overwrites: JArray option
    name: string option
    topic: string option
    nsfw: bool option
    last_message_id: string option
    bitrate: int option
    user_limit: int option
    recipients: JArray option
    icon: string option
    owner_id: string option
    application_id: string option
    parent_id: string option
    last_pin_timestamp: string option
  }

  member this.ToReal () : Channel =
    {
      Id = this.id |> snowflake
      Type = this.``type``
      GuildId = this.guild_id |> Option.map (snowflake)
      Position = this.position
      PermissionOverwrites = this.permission_overwrites |> Option.map (fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> Overwrite.Deserialize) |> Seq.toArray)
      Name = this.name
      Topic = this.topic
      Nsfw = this.nsfw
      LastMessageId = this.last_message_id |> Option.map (snowflake)
      Bitrate = this.bitrate
      UserLimit = this.user_limit
      Recipients = this.recipients |> Option.map (fun jarr -> jarr.Values() |> Seq.map (fun x -> x.ToString() |> User.Deserialize) |> Seq.toArray)
      Icon = this.icon
      OwnerId = this.owner_id |> Option.map (snowflake)
      ApplicationId = this.application_id |> Option.map (snowflake)
      ParentId = this.parent_id |> Option.map (snowflake)
      LastPinTimestamp = this.last_pin_timestamp |> Option.map (fun s -> DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind))
    }

  static member FromReal (x : Channel) : ChannelJson =
    {
      id = x.Id |> fun x -> x.ToString()
      ``type`` = x.Type
      guild_id = x.GuildId |> Option.map (fun x -> x.ToString())
      position = x.Position
      permission_overwrites = x.PermissionOverwrites |> Option.map (Seq.map (fun x -> x.Serialize) >> Seq.toArray >> JArray)
      name = x.Name
      topic = x.Topic
      nsfw = x.Nsfw
      last_message_id = x.LastMessageId |> Option.map (fun x -> x.ToString())
      bitrate = x.Bitrate
      user_limit = x.UserLimit
      recipients = x.Recipients |> Option.map (Seq.map (fun x -> x.Serialize) >> Seq.toArray >> JArray)
      icon = x.Icon
      owner_id = x.OwnerId |> Option.map (fun x -> x.ToString())
      application_id = x.ApplicationId |> Option.map (fun x -> x.ToString())
      parent_id = x.ParentId |> Option.map (fun x -> x.ToString())
      last_pin_timestamp = x.LastPinTimestamp |> Option.map (fun d -> d.ToString("s", System.Globalization.CultureInfo.InvariantCulture))
    }

type Channel =
  {
    /// <summary>the id of this channel</summary>
    Id: Snowflake
    /// <summary>the type of channel</summary>
    Type: int
    /// <summary>the id of the guild</summary>
    GuildId: Snowflake option
    /// <summary>sorting position of the channel</summary>
    Position: int option
    /// <summary>explicit permission overwrites for members and roles</summary>
    PermissionOverwrites: Overwrite array option
    /// <summary>the name of the channel (2-100 characters)</summary>
    Name: string option
    /// <summary>the channel topic (0-1024 characters)</summary>
    Topic: string option
    /// <summary>if the channel is nsfw</summary>
    Nsfw: bool option
    /// <summary>the id of the last message sent in this channel (may not point to an existing or valid message)</summary>
    LastMessageId: Snowflake option
    /// <summary>the bitrate (in bits) of the voice channel</summary>
    Bitrate: int option
    /// <summary>the user limit of the voice channel</summary>
    UserLimit: int option
    /// <summary>the recipients of the DM</summary>
    Recipients: User array option
    /// <summary>icon hash</summary>
    Icon: string option
    /// <summary>id of the DM creator</summary>
    OwnerId: Snowflake option
    /// <summary>application id of the group DM creator if it is bot-created</summary>
    ApplicationId: Snowflake option
    /// <summary>id of the parent category for a channel</summary>
    ParentId: Snowflake option
    /// <summary>when the last pinned message was pinned</summary>
    LastPinTimestamp: DateTime option
  }

  static member Deserialize str =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = JsonConvert.DeserializeObject<ChannelJson>(str, opts)
    intermed.ToReal ()

  member this.Serialize () =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = ChannelJson.FromReal this
    JsonConvert.SerializeObject(intermed, opts)