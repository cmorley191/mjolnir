using Discord.Structures;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Http {
    public partial class HttpBotInterface {
        public async Task<User> GetUser(long id) =>
            User.Deserialize(await MakeRequest(HttpMethod.Get, $"users/{id}"));

        public async Task<User> GetMe() =>
            User.Deserialize(await MakeRequest(HttpMethod.Get, $"users/@me"));

        public async Task<string> Update_Self(string data) =>
            await MakeRequest(new HttpMethod("PATCH"), "users/@me", json: data);
    }

}
