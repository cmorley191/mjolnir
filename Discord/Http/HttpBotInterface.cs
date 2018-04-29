using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Structures;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Linq;

namespace Discord.Http {
    public partial class HttpBotInterface {
        private static readonly HttpClient client = new HttpClient();
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

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", authorizationToken);

            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            client.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot (https://github.com/cmorley191/mjolnir, {version})");
        }

        public async Task<string> MakeRequest(HttpMethod method, string resource, IDictionary<string, string> queryParameters = null) {
            var uriBuilder = new UriBuilder(new Uri(baseUrl, resource));

            if (queryParameters != null) {
                // src: https://stackoverflow.com/a/7230446/2343795
                var uriQueryParameters = HttpUtility.ParseQueryString(uriBuilder.Query);
                foreach (var inputPair in queryParameters)
                    uriQueryParameters[inputPair.Key] = inputPair.Value;
                uriBuilder.Query = uriQueryParameters.ToString();
            }

            var message = new HttpRequestMessage(method, uriBuilder.Uri);

            var response = await client.SendAsync(message);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"{response.StatusCode}: {responseString}");

            return responseString;
        }

        private IDictionary<string, string> collectQueryParameters<TKey, TValue>(params (TKey, TValue)[] entries) =>
            entries
                .Where(tpl => tpl.Item2 != null)
                .ToDictionary(tpl => tpl.Item1.ToString(), tpl => tpl.Item2.ToString());
    }
}