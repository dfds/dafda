using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Tests.TestDoubles
{
    public class ProducerSpy : IProducer
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