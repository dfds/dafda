using System.Threading.Tasks;

namespace Dafda.Producing
{
    public class Producer : IProducer
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly OutgoingMessageFactory _outgoingMessageFactory;

        public Producer(IKafkaProducer kafkaProducer, IOutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
        {
            _kafkaProducer = kafkaProducer;
            _outgoingMessageFactory = new OutgoingMessageFactory(outgoingMessageRegistry, messageIdGenerator);
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