using Confluent.Kafka;
using Dafda.Configuration;

namespace Dafda.Producing
{
    public class KafkaProducerFactory
    {
        public IProducer CreateProducer(IProducerConfiguration configuration)
        {
            var producer = new ProducerBuilder<string, string>(configuration).Build();

            return new KafkaProducer(producer);
        }
    }
}