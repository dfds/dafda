using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Dafda.Producing
{
    internal class KafkaProducer : IDisposable
    {
        private readonly ILogger<KafkaProducer> _logger;
        private readonly IProducer<string, string> _innerKafkaProducer;
        private readonly IPayloadSerializer _serializer;

        public KafkaProducer(ILoggerFactory loggerFactory, IEnumerable<KeyValuePair<string, string>> configuration, IPayloadSerializer payloadSerializer)
        {
            _logger = loggerFactory.CreateLogger<KafkaProducer>();
            _innerKafkaProducer = new ProducerBuilder<string, string>(configuration).Build();
            _serializer = payloadSerializer;
        }

        public async Task Produce(PayloadDescriptor payloadDescriptor)
        {
            await InternalProduce(
                topic: payloadDescriptor.TopicName,
                key: payloadDescriptor.PartitionKey,
                value: await _serializer.Serialize(payloadDescriptor)
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