using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Producing;
using Dafda.Serializing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dafda.Tests.TestDoubles
{
    internal class KafkaProducerSpy : KafkaProducer
    {
        private readonly List<string> _keys = new List<string>();
        private readonly List<ProducedMessage> _producedMessages = new List<ProducedMessage>();

        public KafkaProducerSpy()
            : this(Enumerable.Empty<KeyValuePair<string, string>>(), new DefaultPayloadSerializer())
        {
        }

        public KafkaProducerSpy(IPayloadSerializer payloadSerializer)
            : this(Enumerable.Empty<KeyValuePair<string, string>>(), payloadSerializer)
        {
        }

        private KafkaProducerSpy(IEnumerable<KeyValuePair<string, string>> configuration, IPayloadSerializer payloadSerializer)
            : base(NullLoggerFactory.Instance, configuration, new TopicPayloadSerializerRegistry(() => payloadSerializer))
        {
        }

        internal override Task<DeliveryResult<string, string>> InternalProduce(string topic, string key, string value)
        {
            Topic = topic;
            Key = key;
            Value = value;
            ProducerActivityId = Activity.Current?.Id;
            ProduceCallCount++;
            _keys.Add(key);
            _producedMessages.Add(new ProducedMessage
            {
                Topic = topic,
                Key = key,
                Value = value,
                Order = ProduceCallCount
            });

            var deliveryResult = new DeliveryResult<string, string>
            {
                Topic = topic,
                Partition = new Partition(0),
                Offset = new Offset(ProduceCallCount - 1),
                Message = new Message<string, string>
                {
                    Key = key,
                    Value = value
                }
            };

            return Task.FromResult(deliveryResult);
        }

        public override void Dispose()
        {
            base.Dispose();
            WasDisposed = true;
        }

        public bool WasDisposed { get; private set; }

        public string Topic { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }
        public string ProducerActivityId { get; private set; }
        public int ProduceCallCount { get; private set; }
        public IReadOnlyList<string> AllKeys => _keys;
        public IReadOnlyList<ProducedMessage> ProducedMessages => _producedMessages;

        public class ProducedMessage
        {
            public string Topic { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
            public int Order { get; set; }
        }
    }
}