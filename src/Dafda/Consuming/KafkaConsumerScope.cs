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
        private readonly string _groupId;

        internal KafkaConsumerScope(ILoggerFactory loggerFactory, IConsumer<string, string> innerKafkaConsumer, IIncomingMessageFactory incomingMessageFactory, string groupId)
        {
            _logger = loggerFactory.CreateLogger<KafkaConsumerScope>();
            _innerKafkaConsumer = innerKafkaConsumer;
            _incomingMessageFactory = incomingMessageFactory;
            _groupId = groupId;
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
                })
            {
                Topic = innerResult.Topic,
                Partition = innerResult.Partition.Value,
                PartitionKey = innerResult.Message.Key,
                ClientId = _innerKafkaConsumer.Name,
                GroupId = _groupId
            };

            return Task.FromResult(result);
        }

        public override void Dispose()
        {
            _innerKafkaConsumer.Close();
            _innerKafkaConsumer.Dispose();
        }
    }
}