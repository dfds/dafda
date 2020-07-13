using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class KafkaConsumerScope : ConsumerScope
    {
        private readonly ILogger<KafkaConsumerScope> _logger;
        private readonly IConsumer<string, string> _innerKafkaConsumer;
        private readonly IIncomingMessageFactory _incomingMessageFactory;

        internal KafkaConsumerScope(ILoggerFactory loggerFactory, IConsumer<string, string> innerKafkaConsumer, IIncomingMessageFactory incomingMessageFactory)
        {
            _logger = loggerFactory.CreateLogger<KafkaConsumerScope>();
            _innerKafkaConsumer = innerKafkaConsumer;
            _incomingMessageFactory = incomingMessageFactory;
        }

        public override Task<MessageResult> GetNext(CancellationToken cancellationToken)
        {
            var innerResult = _innerKafkaConsumer.Consume(cancellationToken);

            _logger.LogDebug("Received message {Key}: {RawMessage}", innerResult.Message?.Key, innerResult.Message?.Value);

            var result = new MessageResult(
                message: _incomingMessageFactory.Create(innerResult.Message.Value),
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