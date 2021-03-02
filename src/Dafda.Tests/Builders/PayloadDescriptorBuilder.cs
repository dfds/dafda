using System.Collections.Generic;
using System.Linq;
using Dafda.Producing;
using Dafda.Serializing;

namespace Dafda.Tests.Builders
{
    public class PayloadDescriptorBuilder
    {
        private string _messageId;
        private string _topicName;
        private string _partitionKey;
        private string _messageType;
        private object _messageData;
        private IEnumerable<KeyValuePair<string, string>> _messageHeaders;

        public PayloadDescriptorBuilder()
        {
            _messageId = "dummy-message-id";
            _topicName = "dummy-topic-name";
            _partitionKey = "dummy-partition-key";
            _messageType = "dummy-message-type";
            _messageData = "dummy-message-data";
            _messageHeaders = Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public PayloadDescriptorBuilder WithMessageId(string messageId)
        {
            _messageId = messageId;
            return this;
        }

        public PayloadDescriptorBuilder WithTopicName(string topicName)
        {
            _topicName = topicName;
            return this;
        }

        public PayloadDescriptorBuilder WithPartitionKey(string partitionKey)
        {
            _partitionKey = partitionKey;
            return this;
        }

        public PayloadDescriptorBuilder WithMessageType(string messageType)
        {
            _messageType = messageType;
            return this;
        }

        public PayloadDescriptorBuilder WithMessageData(object messageData)
        {
            _messageData = messageData;
            return this;
        }

        public PayloadDescriptorBuilder WithMessageHeaders(params KeyValuePair<string, string>[] messageHeaders)
        {
            _messageHeaders = messageHeaders;
            return this;
        }

        public PayloadDescriptor Build()
        {
            return new PayloadDescriptor(
                messageId: _messageId,
                topicName: _topicName,
                partitionKey: _partitionKey,
                messageType: _messageType,
                messageData: _messageData,
                messageHeaders: _messageHeaders
            );
        }
    }
}