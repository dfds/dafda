using System.Threading.Tasks;
using Dafda.Outbox;

namespace Dafda.Producing
{
    public sealed class OutboxProducer
    {
        private readonly KafkaProducer _kafkaProducer;

        internal OutboxProducer(KafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
        }

        public async Task Produce(OutboxMessage message)
        {
            var outgoingMessage = BuildOutgoingMessage(message);
            await _kafkaProducer.Produce(outgoingMessage);
        }

        private static OutgoingMessage BuildOutgoingMessage(OutboxMessage message)
        {
            return new OutgoingMessageBuilder()
                .WithTopic(message.Topic)
                .WithMessageId(message.MessageId.ToString())
                .WithKey(message.Key)
                .WithValue(message.Data)
                .WithType(message.Type)
                .Build();
        }
    }
}