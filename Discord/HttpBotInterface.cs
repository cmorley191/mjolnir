
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Discord {
  public class HttpBotInterface {

    private string authorizationToken;
    private Uri baseUrl;

    private static readonly HttpClient client = new HttpClient();

    public HttpBotInterface(string authorizationToken = null, string baseUrl = null) {
      this.authorizationToken = authorizationToken ?? Environment.GetEnvironmentVariable("DISCORD_TOKEN");
      this.baseUrl = new Uri(baseUrl ?? Environment.GetEnvironmentVariable("BASE_URL"));
    }

    public async Task<string> MakeRequest(HttpMethod method = null, string resource = "") {
      method = method ?? HttpMethod.Get;

      var message = new HttpRequestMessage(method, new Uri(baseUrl, resource));
      message.Headers.Authorization = new AuthenticationHeaderValue("Bot", authorizationToken);
      message.Headers.Add("User-Agent", "DiscordBot (https://github.com/cmorley191/mjolnir, v1.0.0)");

      var response = await client.SendAsync(message);
      var responseString = await response.Content.ReadAsStringAsync();

      if (!response.IsSuccessStatusCode)
        throw new Exception($"{response.StatusCode}: {responseString}");

      return responseString;
    }
  }
}
