using System;
using Dafda.Producing;

namespace Dafda.Configuration
{
    public sealed class ProducerTopicRegistration
    {
        private readonly OutgoingMessageRegistry _registry;
        private readonly string _topic;

        internal ProducerTopicRegistration(OutgoingMessageRegistry registry, string topic)
        {
            _registry = registry;
            _topic = topic;
        }

        public ProducerTopicRegistration Register<TMessage>(string type, Func<TMessage, string> keySelector) where TMessage : class
        {
            _registry.Register(_topic, type, keySelector);
            return this;
        }

        public ProducerTopicRegistrationBaseOn<T> BaseOn<T>(Func<T, string> keySelector)
        {
            return new ProducerTopicRegistrationBaseOn<T>(_registry, _topic, keySelector);
        }

        public sealed class ProducerTopicRegistrationBaseOn<TMessageBase>
        {
            private readonly OutgoingMessageRegistry _registry;
            private readonly string _topic;
            private readonly Func<TMessageBase, string> _keySelector;

            internal ProducerTopicRegistrationBaseOn(OutgoingMessageRegistry registry, string topic, Func<TMessageBase, string> keySelector)
            {
                _registry = registry;
                _topic = topic;
                _keySelector = keySelector;
            }

            public ProducerTopicRegistrationBaseOn<TMessageBase> Register<TMessage>(string type) where TMessage : class, TMessageBase
            {
                _registry.Register<TMessage>(_topic, type, _keySelector);
                return this;
            }

            public ProducerTopicRegistrationBaseOn<TMessageBase> Register<TMessage>(string type, Func<TMessageBase, string> keySelector) where TMessage : class, TMessageBase
            {
                _registry.Register<TMessage>(_topic, type, keySelector);
                return this;
            }
        }
    }
}