using System.Threading.Tasks;

namespace Dafda.Producing
{
    public sealed class Producer
    {
        private readonly KafkaProducer _kafkaProducer;
        private readonly OutgoingMessageFactory _outgoingMessageFactory;

        internal Producer(KafkaProducer kafkaProducer, OutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
        {
            _kafkaProducer = kafkaProducer;
            _outgoingMessageFactory = new OutgoingMessageFactory(outgoingMessageRegistry, messageIdGenerator);
        }

        internal string Name { get; set; } = "__Default Producer__";
        
        public async Task Produce(object message)
        {
            var outgoingMessage = _outgoingMessageFactory.Create(message);
            await _kafkaProducer.Produce(outgoingMessage);
        }
    }
}