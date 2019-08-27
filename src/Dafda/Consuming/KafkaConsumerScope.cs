using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Messaging;

namespace Dafda.Consuming
{
    internal class KafkaConsumerScope : TopicSubscriberScope
    {
        private readonly IConsumer<string, string> _innerKafkaConsumer;

        internal KafkaConsumerScope(IConsumer<string, string> innerKafkaConsumer)
        {
            _innerKafkaConsumer = innerKafkaConsumer;
        }
        
        public override Task<MessageResult> GetNext(CancellationToken cancellationToken)
        {
            var innerResult = _innerKafkaConsumer.Consume(cancellationToken);

            var result = new MessageResult(
                message: new JsonMessageEmbeddedDocument(innerResult.Value),
                onCommit: () =>
                {
                    _innerKafkaConsumer.Commit(innerResult);
                    return Task.CompletedTask;
                });

            return Task.FromResult(result);
        }

        public override void Dispose()
        {
            _innerKafkaConsumer.Close();
            _innerKafkaConsumer.Dispose();
        }
    }
}