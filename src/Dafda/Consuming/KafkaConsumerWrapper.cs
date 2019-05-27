using System.Threading;
using Confluent.Kafka;

namespace Dafda.Consuming
{
    internal class KafkaConsumerWrapper : IConsumer
    {
        private readonly IConsumer<string, string> _kafkaConsumer;

        internal KafkaConsumerWrapper(IConsumer<string, string> kafkaConsumer)
        {
            _kafkaConsumer = kafkaConsumer;
        }
        
        public ConsumeResult Consume(CancellationToken cancellationToken)
        {
            var result = _kafkaConsumer.Consume(cancellationToken);
            return new ConsumeResult(result);
        }

        public void Commit(ConsumeResult result)
        {
            _kafkaConsumer.Commit(result.InnerResult);
        }

        public void Dispose()
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
        }
    }
}