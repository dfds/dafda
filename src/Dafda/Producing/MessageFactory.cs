using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;

namespace Dafda.Producing
{
    public static class MessageFactory
    {
        public static Message<string, string> Create(OutgoingMessage outgoingMessage)
        {
            return new Message<string, string>
            {
                Key = outgoingMessage.Key,
                Headers = CreateHeaders(outgoingMessage.Headers),
                Value = outgoingMessage.RawMessage
            };
        }

        private static Headers CreateHeaders(IDictionary<string, string> outgoingMessageHeaders)
        {
            var headers = new Headers();

            foreach (var keyValuePair in outgoingMessageHeaders)
            {
                var header = new Header(keyValuePair.Key, Encoding.ASCII.GetBytes(keyValuePair.Value));
                headers.Add(header);
            }

            return headers;
        }
    }
}