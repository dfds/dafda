using Confluent.Kafka;
using Dafda.Configuration;

namespace Dafda.Producing
{
    public static class KafkaProducerFactory
    {
        public static IProducer CreateProducer(IProducerConfiguration configuration)
        {
            var producer = new ProducerBuilder<string, string>(configuration).Build();

            return new KafkaProducer(producer);
        }
    }
}