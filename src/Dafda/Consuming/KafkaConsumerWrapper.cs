using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Messaging;

namespace Dafda.Consuming
{
    internal class KafkaConsumerWrapper : IInternalConsumer
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
                message: new JsonMessageEmbeddedDocument(result.Value),
                onCommit: () =>
                {
                    _kafkaConsumer.Commit(result);
                    return Task.CompletedTask;
                });
        }

        public void Dispose()
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
        }
    }
}