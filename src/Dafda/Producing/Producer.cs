using System.Threading.Tasks;
using Dafda.Logging;

namespace Dafda.Producing
{
    public class Producer : IProducer
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly IKafkaProducer _kafkaProducer;
        private readonly IOutgoingMessageRegistry _outgoingMessageRegistry;
        private readonly MessageIdGenerator _messageIdGenerator;

        public Producer(IKafkaProducer kafkaProducer, IOutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
        {
            _kafkaProducer = kafkaProducer;
            _outgoingMessageRegistry = outgoingMessageRegistry;
            _messageIdGenerator = messageIdGenerator;
        }

        public async Task Produce(object message)
        {
            var registration = _outgoingMessageRegistry.GetRegistration(message);
            var messageId = _messageIdGenerator.NextMessageId();
            var rawMessage = SerializeRawMessage(message, messageId, registration);

            var outgoingMessage = new OutgoingMessageBuilder()
                .WithTopic(registration.Topic)
                .WithMessageId(messageId)
                .WithKey(registration.KeySelector(message))
                .WithValue(rawMessage)
                .WithType(registration.Type)
                .Build();

            await _kafkaProducer.Produce(outgoingMessage);
        }

        private static string SerializeRawMessage(object message, string messageId, OutgoingMessageRegistration registration)
        {
            return MessageEnvelopeSerializer.Create(new MessageEnvelope(messageId, registration.Type, message));
        }

        public void Dispose()
        {
            _kafkaProducer?.Dispose();
        }
    }
}