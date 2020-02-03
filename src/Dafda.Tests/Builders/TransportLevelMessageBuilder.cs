using Dafda.Consuming;
using Dafda.Tests.TestDoubles;

namespace Dafda.Tests.Builders
{
    public class TransportLevelMessageBuilder
    {
        private string _messageId;
        private string _correlationId;
        private string _type;
        private object _data;

        public TransportLevelMessageBuilder()
        {
            _messageId = "foo-message-id";
            _correlationId = "foo-correlation-id";
            _type = "foo-type";
            _data = "foo-data";
        }

        public TransportLevelMessageBuilder WithType(string type)
        {
            _type = type;
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
                MessageId = _messageId,
                CorrelationId = _correlationId,
                Type = _type,
                Data = _data
            };
        }
    }
}