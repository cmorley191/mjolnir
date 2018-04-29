using Discord.Structures;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Http {
    public partial class HttpBotInterface {
        public async Task<Channel> GetChannel(long id) =>
            Channel.Deserialize(await MakeRequest(HttpMethod.Get, $"channels/{id}"));

        private string queryBuilder(string uri, List<string> queryParams) {
            if (queryParams.Count <= 0) return uri;
            return uri + "?" + string.Join('&', queryParams);
        }

        public Task<Message> GetMessage(Channel channel, long messageId) => GetMessage(channel.Id, messageId);
        public async Task<Message> GetMessage(long channelId, long messageId) =>
            Message.Deserialize(await MakeRequest(HttpMethod.Get, $"channels/{channelId}/messages/{messageId}"));

        public async Task<Message[]> GetMessages(long channelId, int limit = -1, long aroundMessage = -1, long beforeMessage = -1, long afterMessage = -1) {
            var baseUri = $"channels/{channelId}/messages";
            List<string> queryParams = new List<string>();

            if (limit >= 0) queryParams.Add($"limit={limit}");

            //These three are Mutually Exclusive
            if (aroundMessage >= 0) {
                queryParams.Add($"around={aroundMessage}");
            } else if (beforeMessage >= 0) {
                queryParams.Add($"before={beforeMessage}");
            } else if (afterMessage >= 0) {
                queryParams.Add($"after={afterMessage}");
            }

            Console.WriteLine(queryBuilder(baseUri, queryParams));

            return General.DeserializeMany<Message>(await MakeRequest(HttpMethod.Get, queryBuilder(baseUri, queryParams)));
        }
    }
}
