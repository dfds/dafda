using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Configuration;
using Dafda.Logging;
using Dafda.Messaging;

namespace Dafda.Consuming
{
    internal class KafkaConsumerScope : TopicSubscriberScope
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly IConsumer<string, string> _innerKafkaConsumer;
        private readonly IConsumerConfiguration _consumerConfiguration;

        internal KafkaConsumerScope(IConsumer<string, string> innerKafkaConsumer, IConsumerConfiguration consumerConfiguration)
        {
            _innerKafkaConsumer = innerKafkaConsumer;
            _consumerConfiguration = consumerConfiguration;
        }
        
        public override Task<MessageResult> GetNext(CancellationToken cancellationToken)
        {
            var innerResult = _innerKafkaConsumer.Consume(cancellationToken);
            Log.Debug("Received message {Key}: {RawMessage}", innerResult.Key, innerResult.Value);

            var result = new MessageResult(
                message: new JsonMessageEmbeddedDocument(innerResult.Value),
                onCommit: () =>
                {
                    if (_consumerConfiguration.EnableAutoCommit)
                    { 
                        return Task.CompletedTask;
                    }
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