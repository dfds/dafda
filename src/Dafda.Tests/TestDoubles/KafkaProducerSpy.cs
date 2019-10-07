using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Tests.TestDoubles
{
    public class KafkaProducerSpy : IKafkaProducer
    {
        public Task Produce(OutgoingMessage outgoingMessage)
        {
            LastOutgoingMessage = outgoingMessage;
            return Task.CompletedTask;
        }

        public OutgoingMessage LastOutgoingMessage { get; private set; }

        public void Dispose()
        {
        }
    }
}