using Confluent.Kafka;
using Dafda.Configuration;

namespace Dafda.Producing
{
    public class KafkaProducerFactory : IKafkaProducerFactory
    {
        public IKafkaProducer CreateProducer(IProducerConfiguration configuration)
        {
            var producer = new ProducerBuilder<string, string>(configuration).Build();

            return new KafkaProducer(producer);
        }
    }
}