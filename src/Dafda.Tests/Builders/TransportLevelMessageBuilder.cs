using Dafda.Consuming;

namespace Dafda.Tests.Builders
{
    public class TransportLevelMessageBuilder
    {
        private Metadata _metadata;
        private object _data;
        private string _topic;
        private string _partitionKey;

        public TransportLevelMessageBuilder()
        {
            WithType("foo-type");
            _data = null;
        }

        public TransportLevelMessageBuilder WithType(string type)
        {
            _metadata = new Metadata()
            {
                MessageId = "foo-message-id",
                CorrelationId = "foo-correlation-id",
                Type = type
            };
            return this;
        }

        public TransportLevelMessageBuilder WithData(object data)
        {
            _data = data;
            return this;
        }

        public TransportLevelMessageBuilder WithTopic(string topic)
        {
            _topic = topic;
            return this;
        }

        public TransportLevelMessageBuilder WithPartitionKey(string partitionKey)
        {
            _partitionKey = partitionKey;
            return this;
        }

        public TransportLevelMessage Build()
        {
            return new TransportLevelMessage(_metadata, type => _data)
            {
                Topic = _topic,
                PartitionKey = _partitionKey
            };
        }
    }
}