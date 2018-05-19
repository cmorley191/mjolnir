using Discord.Gateway.Models;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Discord.Gateway {
    public abstract class Gateway {

        protected Logger _log = LogManager.GetCurrentClassLogger();
        private const int MaxRetries = 3;

        protected ClientWebSocket socket = null;

        /// <summary>
        /// Tries to connect to the websocket to Discord and retries on errors.
        /// </summary>
        public async Task TryConnect() {
            for (var i = 0; i < MaxRetries; i++)
                try {
                    await Connect();
                    return;
                } catch (Exception e) {
                    if (i >= MaxRetries)
                        throw e;
                    else
                        _log.Debug("Connection failed. Retrying...");
                }
        }

        /// <summary>
        /// Connects this instance.
        /// </summary>
        public abstract Task Connect();

        internal delegate string PayloadGenerator();
        internal class OutgoingMessage {
            /// <summary>
            /// A method for generating the message - some messages depend on state which may changed while a message sits in the send queue (such as the sequence number),
            /// so this can't be a constant string for those messages (such as heartbeat, resume).
            /// </summary>
            public readonly PayloadGenerator GetPayload;
            public readonly TaskCompletionSource<bool> MessageSent = new TaskCompletionSource<bool>();

            public OutgoingMessage(PayloadGenerator PayloadGenerator) {
                this.GetPayload = PayloadGenerator;
            }
        }

        private BufferBlock<OutgoingMessage> sendQueue = new BufferBlock<OutgoingMessage>();

        private static readonly UTF8Encoding messageEncoding = new UTF8Encoding();

        /// <summary>
        /// Loop which sends messages that appear in the sendQueue.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        internal async Task SendMessagesAgentAsync() {
            while (true) {
                var message = await sendQueue.ReceiveAsync();
                var messageData = message.GetPayload();
                var sendBuffer = messageEncoding.GetBytes(messageData);
                //_log.Debug($"Sending: {messageData}");
                await socket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                _log.Debug($"{this.GetType().Name} Sent: {messageData}");
                message.MessageSent.SetResult(true);
            }
        }

        /// <summary>
        /// Add a message to the queue and wait for it to be sent.
        /// </summary>
        /// <param name="payloadGenerator"></param>
        /// <returns></returns>
        internal Task SendMessage(PayloadGenerator payloadGenerator) {
            var message = new OutgoingMessage(payloadGenerator);
            sendQueue.Post(message);
            return message.MessageSent.Task;
        }

        /// <summary>
        /// Add a message to the queue and wait for it to be sent.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        internal Task SendMessage(string payload) => SendMessage(() => payload);

        /// <summary>
        /// Receive and parse a single message and return it.
        /// </summary>
        internal async Task<Response> ReceiveMessage() {
            var messageBuilder = new StringBuilder();
            WebSocketReceiveResult receiveResult;
            do {
                receiveResult = null;

                var buffer = new ArraySegment<byte>(new byte[4 * 1024]);
                receiveResult = await socket.ReceiveAsync(buffer, CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer.Array, 0, receiveResult.Count);
                messageBuilder.Append(message);
            } while (!receiveResult.EndOfMessage || messageBuilder.ToString().Trim() == "");

            var result = messageBuilder.ToString();

            _log.Debug($"{this.GetType().Name} Received: {result}");
            return JsonConvert.DeserializeObject<Response>(result);
        }

        /// <summary>
        /// Loop which receives messages and passes them off to the appropriate destination (usually a handler).
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="readyResponseSource">Source for receiving the single Ready response to the Identify command.</param>
        /// <returns></returns>
        internal async Task ReceiveMessagesAgentAsync() {
            while (true) {
                var response = await ReceiveMessage();
                await HandleResponse(response);
            }
        }

        internal abstract Task HandleResponse(Response response);
    }
}
