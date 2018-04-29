using Discord.Structures;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Http {
    public partial class HttpBotInterface {
        public async Task<Guild> GetGuild(long id) =>
            Guild.Deserialize(await MakeRequest(HttpMethod.Get, $"guilds/{id}"));

        public async Task<Guild[]> GetAccessibleGuilds() =>
            General.DeserializeMany<Guild>(await MakeRequest(HttpMethod.Get, "users/@me/guilds"));

        public Task<Channel[]> GetGuildChannels(Guild guild) => GetGuildChannels(guild.Id);
        public async Task<Channel[]> GetGuildChannels(long guildId) =>
            General.DeserializeMany<Channel>(await MakeRequest(HttpMethod.Get, $"guilds/{guildId}/channels"));

    }
}
