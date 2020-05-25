using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Serializing;
using Microsoft.Extensions.Logging;

namespace Dafda.Producing
{
    internal class KafkaProducer : IDisposable
    {
        private readonly TopicPayloadSerializerRegistry _payloadSerializerRegistry;
        private readonly ILogger<KafkaProducer> _logger;
        private readonly IProducer<string, string> _innerKafkaProducer;

        public KafkaProducer(ILoggerFactory loggerFactory, IEnumerable<KeyValuePair<string, string>> configuration, TopicPayloadSerializerRegistry payloadSerializerRegistry)
        {
            _payloadSerializerRegistry = payloadSerializerRegistry;
            _logger = loggerFactory.CreateLogger<KafkaProducer>();
            _innerKafkaProducer = new ProducerBuilder<string, string>(configuration).Build();
        }

        public async Task Produce(PayloadDescriptor payloadDescriptor)
        {
            var serializer = _payloadSerializerRegistry.Get(payloadDescriptor.TopicName);
            
            await InternalProduce(
                topic: payloadDescriptor.TopicName,
                key: payloadDescriptor.PartitionKey,
                value: await serializer.Serialize(payloadDescriptor)
            );
        }

        internal virtual async Task InternalProduce(string topic, string key, string value)
        {
            try
            {
                _logger.LogDebug("Producing message with {Key} on {Topic}", key, topic);

                await _innerKafkaProducer.ProduceAsync(
                    topic: topic,
                    message: new Message<string, string>
                    {
                        Key = key,
                        Value = value
                    }
                );
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError(e, "Error publishing message due to: {ErrorReason} ({ErrorCode})", e.Error.Reason, e.Error.Code);
                throw;
            }
        }

        public virtual void Dispose()
        {
            _innerKafkaProducer?.Dispose();
        }
    }
}