using System.Threading;
using Dafda.Consuming;
using Dafda.Tests.TestDoubles;

namespace Dafda.Tests.Messaging
{
    public class InternalConsumerStub : IInternalConsumer
    {
        private readonly string _messageType;

        public InternalConsumerStub(string messageType = "foo")
        {
            _messageType = messageType;
        }

        public ConsumeResult Consume(CancellationToken cancellationToken)
        {
            return new ConsumeResult(
                message: new TransportLevelMessageStub(type: _messageType)
            );
        }

        public void Dispose()
        {
            
        }
    }
}