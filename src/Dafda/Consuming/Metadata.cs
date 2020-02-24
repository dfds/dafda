using System.Collections.Generic;

namespace Dafda.Consuming
{
    /// <remarks>
    /// https://blog.arkency.com/correlation-id-and-causation-id-in-evented-systems/
    /// https://codeopinion.com/message-properties/
    /// </remarks>
    public sealed class Metadata
    {
        private readonly IDictionary<string, string> _metadata;

        internal Metadata() : this(new Dictionary<string, string>())
        {
        }

        public Metadata(IDictionary<string, string> metadata)
        {
            _metadata = metadata;
        }

        public string MessageId
        {
            get => this[MessageEnvelopeProperties.MessageId];
            internal set => this[MessageEnvelopeProperties.MessageId] = value;
        }

        public string Type
        {
            get => this[MessageEnvelopeProperties.Type];
            internal set => this[MessageEnvelopeProperties.Type] = value;
        }

        public string CorrelationId
        {
            get => this[MessageEnvelopeProperties.CorrelationId];
            internal set => this[MessageEnvelopeProperties.CorrelationId] = value;
        }

        public string CausationId
        {
            get => this[MessageEnvelopeProperties.CausationId];
            internal set => this[MessageEnvelopeProperties.CausationId] = value;
        }

        public string this[string key]
        {
            get
            {
                _metadata.TryGetValue(key, out var value);
                return value;
            }
            private set => _metadata[key] = value;
        }
    }
}