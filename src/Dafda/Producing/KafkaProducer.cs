using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Logging;

namespace Dafda.Producing
{
    internal class KafkaProducer : IDisposable
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
        private readonly IProducer<string, string> _innerKafkaProducer;
        private readonly IPayloadSerializer _serializer;

        public KafkaProducer(IEnumerable<KeyValuePair<string, string>> configuration, IPayloadSerializer payloadSerializer)
        {
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
                Log.Debug("Producing message with {Key} on {Topic}", key, topic);

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
                Log.Error(e, "Error publishing message due to: {ErrorReason} ({ErrorCode})", e.Error.Reason, e.Error.Code);
                throw;
            }
        }

        public virtual void Dispose()
        {
            _innerKafkaProducer?.Dispose();
        }
    }
}