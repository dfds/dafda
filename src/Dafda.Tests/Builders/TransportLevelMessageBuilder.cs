using Dafda.Consuming;
using Dafda.Tests.TestDoubles;

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
            _data = "foo-data";
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
            return new TransportLevelMessageStub
            {
                Metadata = _metadata,
                Data = _data
            };
        }
    }
}