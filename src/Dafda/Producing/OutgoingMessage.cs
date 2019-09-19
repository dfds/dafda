using System.Collections.Generic;

namespace Dafda.Producing
{
    public class OutgoingMessage
    {
        public const string MessageIdHeaderName = "messageId";
        public const string TypeHeaderName = "type";

        public OutgoingMessage(string topic, string messageId, string key, string type, string rawMessage)
        {
            Topic = topic;
            MessageId = messageId;
            Type = type;
            Key = key;
            RawMessage = rawMessage;

            Headers.Add(MessageIdHeaderName, messageId);
            Headers.Add(TypeHeaderName, type);
        }

        public string Topic { get; }
        public string MessageId { get; }
        public string Type { get; }
        public string Key { get; }
        public string RawMessage { get; }

        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
    }
}