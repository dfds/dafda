using Dafda.Consuming;

namespace Dafda.Tests.Builders
{
    public class TransportLevelMessageBuilder
    {
        private readonly Metadata _metadata = new Metadata();

        private object _data;

        public TransportLevelMessageBuilder()
        {
            _metadata.MessageId = "foo-message-id";
            _metadata.CorrelationId = "foo-correlation-id";
            _metadata.Type = "foo-type";
            _data = null;
        }

        public TransportLevelMessageBuilder WithType(string type)
        {
            _metadata.Type = type;
            return this;
        }

        public TransportLevelMessageBuilder WithData(object data)
        {
            _data = data;
            return this;
        }

        public ITransportLevelMessage Build()
        {
            return new TransportLevelMessage(_metadata, type => _data);
        }
    }
}