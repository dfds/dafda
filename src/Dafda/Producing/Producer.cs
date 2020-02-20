using System.Threading.Tasks;
using Dafda.Outbox;

namespace Dafda.Producing
{
    internal class Producer : IProducer
    {
        private readonly IKafkaProducer _kafkaProducer;
        private readonly OutgoingMessageFactory _outgoingMessageFactory;

        public Producer(IKafkaProducer kafkaProducer, OutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
        {
            _kafkaProducer = kafkaProducer;
            _outgoingMessageFactory = new OutgoingMessageFactory(outgoingMessageRegistry, messageIdGenerator);
        }

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

        public void Dispose()
        {
            _kafkaProducer?.Dispose();
        }
    }
}