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
    }
}
