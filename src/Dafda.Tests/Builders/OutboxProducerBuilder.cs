using Dafda.Producing;

namespace Dafda.Tests.Builders
{
    internal class OutboxProducerBuilder
    {
        private KafkaProducer _kafkaProducer;

        public OutboxProducerBuilder With(KafkaProducer kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
            return this;
        }

        public OutboxProducer Build()
        {
            return new OutboxProducer(_kafkaProducer);
        }

        public static implicit operator OutboxProducer(OutboxProducerBuilder builder)
        {
            return builder.Build();
        }
    }
}