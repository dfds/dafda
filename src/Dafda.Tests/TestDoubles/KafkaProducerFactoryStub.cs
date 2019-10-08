using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Producing.Kafka;

namespace Dafda.Tests.TestDoubles
{
    public class KafkaProducerFactoryStub : IKafkaProducerFactory
    {
        private readonly IKafkaProducer _producer;

        public KafkaProducerFactoryStub(IKafkaProducer producer)
        {
            _producer = producer;
        }

        public IKafkaProducer CreateProducer(IConfiguration configuration)
        {
            return _producer;
        }
    }
}