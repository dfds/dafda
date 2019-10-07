using System.Collections.Generic;

namespace Dafda.Producing
{
    public class OutgoingMessage
    {
        public OutgoingMessage(string topic, string messageId, string key, string type, string value)
        {
            Topic = topic;
            MessageId = messageId;
            Type = type;
            Key = key;
            Value = value;
        }

        public string Topic { get; }
        public string MessageId { get; }
        public string Type { get; }
        public string Key { get; }
        public string Value { get; }
    }
}