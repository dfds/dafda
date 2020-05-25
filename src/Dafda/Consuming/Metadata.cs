using System.Collections.Generic;

namespace Dafda.Consuming
{
    /// <summary>
    /// Contains metadata about a message.
    /// </summary>
    /// <remarks>
    /// For more information about message properties, see:
    /// https://blog.arkency.com/correlation-id-and-causation-id-in-evented-systems/
    /// https://codeopinion.com/message-properties/
    /// </remarks>
    public sealed class Metadata
    {
        private readonly IDictionary<string, string> _metadata;

        internal Metadata() : this(new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Constructor that takes a map of key/value pairs.
        /// </summary>
        /// <param name="metadata">The collection of key/value pairs.</param>
        public Metadata(IDictionary<string, string> metadata)
        {
            _metadata = metadata;
        }

        /// <summary>
        /// The message identifier.
        /// </summary>
        public string MessageId
        {
            get => this[MessageEnvelopeProperties.MessageId];
            internal set => this[MessageEnvelopeProperties.MessageId] = value;
        }

        /// <summary>
        /// The message type.
        /// </summary>
        public string Type
        {
            get => this[MessageEnvelopeProperties.Type];
            internal set => this[MessageEnvelopeProperties.Type] = value;
        }

        /// <summary>
        /// The correlation identifier.
        /// </summary>
        public string CorrelationId
        {
            get => this[MessageEnvelopeProperties.CorrelationId];
            internal set => this[MessageEnvelopeProperties.CorrelationId] = value;
        }

        /// <summary>
        /// The causation identifier.
        /// </summary>
        public string CausationId
        {
            get => this[MessageEnvelopeProperties.CausationId];
            internal set => this[MessageEnvelopeProperties.CausationId] = value;
        }

        /// <summary>
        /// Other metadata values are access through this.
        /// </summary>
        /// <param name="key">A metadata key</param>
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