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

        /// <summary>
        /// Returns the messages for a channel.
        /// The before, after, and around keys are mutually exclusive, only one may be passed at a time.
        /// </summary>
        /// <remarks>
        /// If operating on a guild channel, this endpoint requires the 'VIEW_CHANNEL' permission to be present on 
        /// the current user. If the current user is missing the 'READ_MESSAGE_HISTORY' permission in the channel 
        /// then this will return no messages (since they cannot read the message history).
        /// </remarks>
        /// <param name="channelId"></param>
        /// <param name="limit">max number of messages to return (1-100); defaults to 50</param>
        /// <param name="aroundMessage">get messages around this message ID</param>
        /// <param name="beforeMessage">get messages before this message ID</param>
        /// <param name="afterMessage">get messages after this message ID</param>
        /// <returns></returns>
        public async Task<Message[]> GetMessages(long channelId, int? limit = null, long? aroundMessage = null, long? beforeMessage = null, long? afterMessage = null) {
            var queryParameters = collectQueryParameters(
                    ("limit", limit),
                    ("around", aroundMessage),
                    ("before", beforeMessage),
                    ("after", afterMessage)
                );

            return General.DeserializeMany<Message>(await MakeRequest(HttpMethod.Get, $"channels/{channelId}/messages", queryParameters));
        }
    }
}
