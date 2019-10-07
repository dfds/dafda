using Confluent.Kafka;
using Dafda.Configuration;

namespace Dafda.Producing
{
    public static class KafkaProducerFactory
    {
        public static IKafkaProducer CreateProducer(IProducerConfiguration configuration)
        {
            var producer = new ProducerBuilder<string, string>(configuration).Build();

            return new KafkaProducer(producer);
        }
    }
}