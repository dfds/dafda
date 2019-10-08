using System.Threading.Tasks;
using Dafda.Logging;

namespace Dafda.Producing
{
    public class Producer : IProducer
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly OutgoingMessageFactory _outgoingMessageFactory;
        private readonly IKafkaProducer _kafkaProducer;

        public Producer(IKafkaProducer kafkaProducer, OutgoingMessageFactory outgoingMessageFactory)
        {
            _kafkaProducer = kafkaProducer;
            _outgoingMessageFactory = outgoingMessageFactory;
        }

        public async Task Produce(object message)
        {
            var outgoingMessage = _outgoingMessageFactory.Create(message);

            await _kafkaProducer.Produce(outgoingMessage);
        }

        public void Dispose()
        {
            _kafkaProducer?.Dispose();
        }
    }
}