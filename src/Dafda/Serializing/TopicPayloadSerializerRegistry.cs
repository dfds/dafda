using System;
using System.Collections.Generic;

namespace Dafda.Serializing
{
    internal sealed class TopicPayloadSerializerRegistry
    {
        private readonly Dictionary<string, Func<IPayloadSerializer>> _serializerFactories = new Dictionary<string, Func<IPayloadSerializer>>();
        private Func<IPayloadSerializer> _defaultPayloadSerializerFactory;

        public TopicPayloadSerializerRegistry(Func<IPayloadSerializer> defaultPayloadSerializerFactory)
        {
            _defaultPayloadSerializerFactory = defaultPayloadSerializerFactory;
        }

        public void Register(string topic, Func<IPayloadSerializer> serializerFactory)
        {
            _serializerFactories.Add(topic, serializerFactory);
        }

        public void SetDefaultPayloadSerializer(Func<IPayloadSerializer> factory)
        {
            _defaultPayloadSerializerFactory = factory;
        }

        public IPayloadSerializer Get(string topic)
        {
            if (_serializerFactories.TryGetValue(topic, out var factory))
            {
                return factory();
            }

            return _defaultPayloadSerializerFactory();
        }
    }
}