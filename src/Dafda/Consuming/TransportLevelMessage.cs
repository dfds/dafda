using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// The Dafda message representation.
    /// </summary>
    public sealed class TransportLevelMessage
    {
        private readonly Func<Type, object> _deserializer;

        /// <summary>
        /// Create an instance of the <see cref="TransportLevelMessage"/>.
        /// </summary>
        /// <param name="metadata">The message metadata.</param>
        /// <param name="deserializer">The message deserializer factory method.</param>
        public TransportLevelMessage(Metadata metadata, Func<Type, object> deserializer)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        /// <summary>
        /// The message metadata.
        /// </summary>
        public Metadata Metadata { get; }

        internal string Topic { get; set; }
        internal string PartitionKey { get; set; }

        /// <summary>
        /// Return a deserialized instance of the <paramref name="messageInstanceType"/>.
        /// </summary>
        /// <param name="messageInstanceType">The message runtime type</param>
        /// <returns>The deserialized message instance.</returns>
        public object ReadDataAs(Type messageInstanceType)
        {
            return _deserializer(messageInstanceType);
        }
    }
}