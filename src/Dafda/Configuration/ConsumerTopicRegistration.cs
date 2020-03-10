using Dafda.Consuming;

namespace Dafda.Configuration
{
    public sealed class ConsumerTopicRegistration
    {
        private readonly MessageHandlerRegistry _registry;
        private readonly string _topic;

        internal ConsumerTopicRegistration(MessageHandlerRegistry registry, string topic)
        {
            _registry = registry;
            _topic = topic;
        }

        public ConsumerTopicRegistration Register<TMessage, TMessageHandler>(string messageType)
            where TMessage : class, new()
            where TMessageHandler : IMessageHandler<TMessage>
        {
            _registry.Register<TMessage, TMessageHandler>(_topic, messageType);
            return this;
        }
    }
}