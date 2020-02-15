using System.Collections.Generic;
using Confluent.Kafka;

namespace Dafda.Producing.Kafka
{
    public class KafkaProducerFactory : IKafkaProducerFactory
    {
        private readonly IEnumerable<KeyValuePair<string, string>> _configuration;

        public KafkaProducerFactory(IEnumerable<KeyValuePair<string, string>> configuration)
        {
            _configuration = configuration;
        }

        public IKafkaProducer CreateProducer()
        {
            var producer = new ProducerBuilder<string, string>(_configuration).Build();

            return new KafkaProducer(producer);
        }
    }
}