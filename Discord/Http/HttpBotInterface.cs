using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Structures;
using System.Collections.Generic;

namespace Discord.Http {
    public partial class HttpBotInterface {
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

        public async Task<string> MakeRequest(HttpMethod method, string resource) {
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
    }
}