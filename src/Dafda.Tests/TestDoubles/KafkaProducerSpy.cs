using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dafda.Producing;

namespace Dafda.Tests.TestDoubles
{
    internal class KafkaProducerSpy : KafkaProducer
    {
        public KafkaProducerSpy()
            : this(Enumerable.Empty<KeyValuePair<string, string>>(), new DefaultPayloadSerializer())
        {
        }

        public KafkaProducerSpy(IPayloadSerializer payloadSerializer)
            : this(Enumerable.Empty<KeyValuePair<string, string>>(), payloadSerializer)
        {
        }

        public KafkaProducerSpy(IEnumerable<KeyValuePair<string, string>> configuration, IPayloadSerializer payloadSerializer)
            : base(configuration, payloadSerializer)
        {
        }

        internal override Task InternalProduce(string topic, string key, string value)
        {
            Topic = topic;
            Key = key;
            Value = value;

            return Task.CompletedTask;
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
    }
}