module rec Discord.Structures.User

open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Newtonsoft.Json.FSharp
open Discord.Structures.RawTypes

type UserJson =
  {
    id: string
    username: string
    discriminator: string
    avatar: string
    bot: bool option
  }

  member this.ToReal () : User =
    {
      Id = this.id |> snowflake
      Username = this.username
      Discriminator = this.discriminator
      Avatar = this.avatar
      Bot = this.bot
    }

  static member FromReal (x : User) : UserJson =
    {
      id = x.Id |> fun x -> x.ToString()
      username = x.Username
      discriminator = x.Discriminator
      avatar = x.Avatar
      bot = x.Bot
    }

type User =
  {
    /// <summary>the user's id</summary>
    Id: Snowflake
    /// <summary>the user's username; not unique across the platform</summary>
    Username: string
    /// <summary>the user's 4-digit discord-tag</summary>
    Discriminator: string
    /// <summary>the user's?avatar hash</summary>
    Avatar: string
    /// <summary>whether the user belongs to an OAuth2 application</summary>
    Bot: bool option
  }

  static member Deserialize str =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = JsonConvert.DeserializeObject<UserJson>(str, opts)
    intermed.ToReal ()

  member this.Serialize () =
    let opts = Serialisation.extend (JsonSerializerSettings())
    let intermed = UserJson.FromReal this
    JsonConvert.SerializeObject(intermed, opts)