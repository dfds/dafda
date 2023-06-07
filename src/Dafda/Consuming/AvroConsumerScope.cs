using Avro.Specific;
using Confluent.Kafka;
using Dafda.Consuming.Avro;
using Dafda.Consuming.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    internal class AvroConsumerScope<TKey, TValue> : IConsumerScope<MessageResult<TKey, TValue>> where TValue : ISpecificRecord
    {
        private readonly ILogger<AvroConsumerScope<TKey, TValue>> _logger;
        private readonly IConsumer<TKey, TValue> _innerKafkaConsumer;

        public AvroConsumerScope(ILoggerFactory loggerFactory, IConsumer<TKey, TValue> innerKafkaConsumer)
        {
            _logger = loggerFactory.CreateLogger<AvroConsumerScope<TKey, TValue>>();
            _innerKafkaConsumer = innerKafkaConsumer;
        }

        public void Dispose()
        {
            _innerKafkaConsumer.Close();
            _innerKafkaConsumer.Dispose();
        }

        public Task<MessageResult<TKey, TValue>> GetNext(CancellationToken cancellationToken)
        {
            var innerResult = _innerKafkaConsumer.Consume(cancellationToken);

            _logger.LogDebug($"Consumed {innerResult.Message.Value.GetType().Name}. Topic = {innerResult.Topic} Offset = {innerResult.Offset} Partition = {innerResult.Partition} Timestamp = {innerResult.Message.Timestamp.UnixTimestampMs}");
            var metadata = new Avro.MessageMetadata(
                topic: innerResult.Topic,
                partition: innerResult.Partition.Value,
                timestamp: innerResult.Message.Timestamp,
                offset: innerResult.Offset.Value);

            var result = new MessageResult<TKey, TValue>(
                    key: innerResult.Message.Key,
                    value: innerResult.Message.Value,
                    headers: TransformHeaders(innerResult.Message.Headers),
                    metadata: metadata,
                    onCommit: () =>
                    {
                        _innerKafkaConsumer.Commit(innerResult);
                        return Task.CompletedTask;
                    });

            return Task.FromResult(result);
        }

        private List<KeyValuePair<string, byte[]>> TransformHeaders(Headers headers)
        {
            var returnHeaders = new List<KeyValuePair<string, byte[]>>();

            var test = headers.GetEnumerator();

            while (test.MoveNext())
                returnHeaders.Add(new KeyValuePair<string, byte[]>(test.Current.Key, test.Current.GetValueBytes()));

            return returnHeaders;
        }
    }
}
