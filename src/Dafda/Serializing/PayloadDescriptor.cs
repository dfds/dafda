using System.Collections.Generic;

namespace Dafda.Serializing
{
    public sealed class PayloadDescriptor
    {
        public PayloadDescriptor(string messageId, string topicName, string partitionKey, string messageType, object messageData, IEnumerable<KeyValuePair<string, object>> messageHeaders)
        {
            MessageId = messageId;
            TopicName = topicName;
            PartitionKey = partitionKey;
            MessageType = messageType;
            MessageData = messageData;
            MessageHeaders = messageHeaders;
        }

        public string TopicName { get; private set; }
        public string PartitionKey { get; private set; }
        public string MessageId { get; private set; }
        public string MessageType { get; private set; }
        public IEnumerable<KeyValuePair<string, object>> MessageHeaders { get; private set; }
        public object MessageData { get; private set; }
    }
}