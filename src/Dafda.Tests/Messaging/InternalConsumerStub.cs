using System.Threading;
using Dafda.Consuming;

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
            return new ConsumeResult($"{{\"type\": \"{_messageType}\", \"data\": {{\"value\":\"bar\"}}}}");
        }

        public void Dispose()
        {
            
        }
    }
}