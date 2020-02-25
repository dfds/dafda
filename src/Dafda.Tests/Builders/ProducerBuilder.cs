using Dafda.Producing;

namespace Dafda.Tests.Builders
{
    internal class ProducerBuilder
    {
        private KafkaProducer _kafkaProducer;
        private OutgoingMessageRegistry _outgoingMessageRegistry = new OutgoingMessageRegistry();
        private MessageIdGenerator _messageIdGenerator = MessageIdGenerator.Default;

        public ProducerBuilder With(KafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
            return this;
        }

        public ProducerBuilder With(OutgoingMessageRegistry outgoingMessageRegistry)
        {
            _outgoingMessageRegistry = outgoingMessageRegistry;
            return this;
        }

        public ProducerBuilder With(MessageIdGenerator messageIdGenerator)
        {
            _messageIdGenerator = messageIdGenerator;
            return this;
        }

        public Producer Build()
        {
            return new Producer(_kafkaProducer, _outgoingMessageRegistry, _messageIdGenerator);
        }

        public static implicit operator Producer(ProducerBuilder builder)
        {
            return builder.Build();
        }
    }
}