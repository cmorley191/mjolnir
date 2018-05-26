using Discord.Structures;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MjolnirCore;

namespace Discord.Http {
    public partial class HttpBotInterface {
        public async Task<Channel> GetChannel(long id) =>
            Channel.Deserialize(await MakeRequest(HttpMethod.Get, $"channels/{id}"));

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

        private string emojiDescriptor(Emoji emoji) => emoji.Id.Match(some: id => $":{emoji.Name}:{id}", none: () => emoji.Name);

        public Task CreateReaction(Message message, Emoji emoji) => CreateReaction(message, emojiDescriptor(emoji));
        public Task CreateReaction(Message message, string emojiDescriptor) => CreateReaction(message.ChannelId, message.Id, emojiDescriptor);
        public async Task CreateReaction(long channelId, long messageId, string emojiDescriptor) =>
            await MakeRequest(HttpMethod.Put, $"channels/{channelId}/messages/{messageId}/reactions/{emojiDescriptor}/@me");

        public Task DeleteReaction(Message message, Emoji emoji) => DeleteReaction(message.ChannelId, message.Id, emojiDescriptor(emoji));
        public async Task DeleteReaction(long channelId, long messageId, string emojiId) =>
            await MakeRequest(HttpMethod.Delete, $"channels/{channelId}/messages/{messageId}/reactions/{emojiId}/@me");

        public Task CreateMessage(Message message, string data) => CreateMessage(message.ChannelId, message.Id, data);
        public async Task CreateMessage(long channelId, long messageId, string data) =>
            await MakeRequest(HttpMethod.Post, $"channels/{channelId}/messages", json: data);

    }
}
