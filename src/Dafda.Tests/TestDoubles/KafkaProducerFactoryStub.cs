using Dafda.Configuration;
using Dafda.Producing;

namespace Dafda.Tests.TestDoubles
{
    public class KafkaProducerFactoryStub : IKafkaProducerFactory
    {
        private readonly IKafkaProducer _producer;

        public KafkaProducerFactoryStub(IKafkaProducer producer)
        {
            _producer = producer;
        }

        public IKafkaProducer CreateProducer(IProducerConfiguration configuration)
        {
            return _producer;
        }
    }
}