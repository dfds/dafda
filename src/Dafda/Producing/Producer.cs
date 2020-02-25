using System.Threading.Tasks;
using Dafda.Outbox;

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
            var outgoingMessage = AssembleOutgoingMessage(message);
            await _kafkaProducer.Produce(outgoingMessage);
        }

        private OutgoingMessage AssembleOutgoingMessage(object message)
        {
            if (message is OutboxMessage outboxMessage)
            {
                return new OutgoingMessageBuilder()
                    .WithTopic(outboxMessage.Topic)
                    .WithMessageId(outboxMessage.MessageId.ToString())
                    .WithKey(outboxMessage.Key)
                    .WithValue(outboxMessage.Data)
                    .WithType(outboxMessage.Type)
                    .Build();
            }

            return _outgoingMessageFactory.Create(message);
        }
    }
}