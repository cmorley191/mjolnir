using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Structures;

namespace Discord {
    public class HttpBotInterface {
        private static readonly HttpClient Client = new HttpClient();
        private readonly string authorizationToken;
        private readonly Uri baseUrl;

        public HttpBotInterface(string nAuthorizationToken = null, string nBaseUrl = null) {
            authorizationToken = nAuthorizationToken ?? Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            if (authorizationToken is null)
                throw new ArgumentException("No authorization token provided.");


            var uriPath = nBaseUrl ?? Environment.GetEnvironmentVariable("BASE_URL");
            if (uriPath is null)
                throw new ArgumentException("No BaseUrl defined!");

            baseUrl = new Uri(uriPath);
        }

        public async Task<string> MakeRequest(HttpMethod method = null, string resource = "") {
            method = method ?? HttpMethod.Get;

            var message = new HttpRequestMessage(method, new Uri(baseUrl, resource));
            message.Headers.Authorization = new AuthenticationHeaderValue("Bot", authorizationToken);

            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            message.Headers.Add("User-Agent", $"DiscordBot (https://github.com/cmorley191/mjolnir, {version})");

            var response = await Client.SendAsync(message);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response.StatusCode}: {responseString}");

            return responseString;
        }

        public async Task<Guild> GetGuild(long id) => Guild.Deserialize(await MakeRequest(HttpMethod.Get, $"guilds/{id}"));
        public async Task<Guild[]> GetAccessibleGuilds() => General.DeserializeMany<Guild>(await MakeRequest(HttpMethod.Get, "users/@me/guilds"));

        public async Task<Channel> GetChannel(long id) => Channel.Deserialize(await MakeRequest(HttpMethod.Get, $"channels/{id}"));
        public async Task<Channel[]> GetGuildChannels(long guildId) => General.DeserializeMany<Channel>(await MakeRequest(HttpMethod.Get, $"guilds/{guildId}/channels"));
        public Task<Channel[]> GetGuildChannels(Guild guild) => GetGuildChannels(guild.Id);

        public async Task<Message> GetMessage(long channelId, long messageId) => Message.Deserialize(await MakeRequest(HttpMethod.Get, $"channels/{channelId}/messages/{messageId}"));
        public Task<Message> GetMessage(Channel channel, long messageId) => GetMessage(channel.Id, messageId);

        public async Task<User> GetUser(long id) => User.Deserialize(await MakeRequest(HttpMethod.Get, $"users/{id}"));
    }
}