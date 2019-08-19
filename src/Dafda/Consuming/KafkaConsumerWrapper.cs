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

            return new ConsumeResult(
                value: result.Value,
                onCommit: () => _kafkaConsumer.Commit(result)
            );
        }

        public void Dispose()
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
        }
    }
}