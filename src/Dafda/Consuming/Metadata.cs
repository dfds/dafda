using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Initialize an instance of Metadata
        /// </summary>
        public Metadata() : this(new Dictionary<string, string>())
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
            internal init => this[MessageEnvelopeProperties.MessageId] = value;
        }

        /// <summary>
        /// The message type.
        /// </summary>
        public string Type
        {
            get => this[MessageEnvelopeProperties.Type];
            internal init => this[MessageEnvelopeProperties.Type] = value;
        }

        /// <summary>
        /// The correlation identifier.
        /// </summary>
        public string CorrelationId
        {
            get => this[MessageEnvelopeProperties.CorrelationId];
            init => this[MessageEnvelopeProperties.CorrelationId] = value;
        }

        /// <summary>
        /// The causation identifier.
        /// </summary>
        public string CausationId
        {
            get => this[MessageEnvelopeProperties.CausationId];
            init => this[MessageEnvelopeProperties.CausationId] = value;
        }

        /// <summary>
        /// The version identifier
        /// </summary>
        public string Version
        {
            get => this[MessageEnvelopeProperties.Version];
            init => this[MessageEnvelopeProperties.Version] = value;
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

        /// <summary>
        /// Creates an enumerable of headers
        /// </summary>
        /// <returns>Headers</returns>
        public IEnumerable<KeyValuePair<string, string>> AsEnumerable()
        {
            return _metadata.AsEnumerable();
        }
    }
}