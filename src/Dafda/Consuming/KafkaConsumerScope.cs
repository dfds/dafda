using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Logging;

namespace Dafda.Consuming
{
    internal class KafkaConsumerScope : ConsumerScope
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly IConsumer<string, string> _innerKafkaConsumer;
        private readonly IncomingMessageFactory _incomingMessageFactory;

        internal KafkaConsumerScope(IConsumer<string, string> innerKafkaConsumer, IncomingMessageFactory incomingMessageFactory)
        {
            _innerKafkaConsumer = innerKafkaConsumer;
            _incomingMessageFactory = incomingMessageFactory;
        }

        public override Task<MessageResult> GetNext(CancellationToken cancellationToken)
        {
            var innerResult = _innerKafkaConsumer.Consume(cancellationToken);

            Log.Debug("Received message {Key}: {RawMessage}", innerResult.Key, innerResult.Value);

            var result = new MessageResult(
                message: _incomingMessageFactory.Create(innerResult.Value),
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