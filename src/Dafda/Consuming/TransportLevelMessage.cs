using System;

namespace Dafda.Consuming
{
    public sealed class TransportLevelMessage
    {
        private readonly Func<Type, object> _deserializer;

        public TransportLevelMessage(Metadata metadata, Func<Type, object> deserializer)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        public Metadata Metadata { get; }

        public object ReadDataAs(Type messageInstanceType)
        {
            return _deserializer(messageInstanceType);
        }
    }
}