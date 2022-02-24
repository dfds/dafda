using Dafda.Consuming;

namespace Dafda.Tests.Builders
{
    public class TransportLevelMessageBuilder
    {
        private string _messageId;
        private string _correlationId;
        private string _type;
        private string _version;

        private object _data;

        public TransportLevelMessageBuilder()
        {
            _messageId = "foo-message-id";
            _correlationId = "foo-correlation-id";
            _type = "foo-type";
            _version = "1";
            _data = null;
        }

        public TransportLevelMessageBuilder WithType(string type)
        {
            _type = type;
            return this;
        }

        public TransportLevelMessageBuilder WithVersion(string version)
        {
            _version = version;
            return this;
        }

        public TransportLevelMessageBuilder WithData(object data)
        {
            _data = data;
            return this;
        }

        public TransportLevelMessage Build()
        {
            var metadata = new Metadata
            {
                MessageId = _messageId,
                CorrelationId = _correlationId,
                Type = _type,
                Version = _version,
            };
            return new TransportLevelMessage(metadata, type => _data);
        }
    }
}