using Dafda.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public interface IMessageHandlerConfiguration
    {
        ITopicMessageHandlerConfiguration FromTopic(string topic);
    }

    public interface ITopicMessageHandlerConfiguration
    {
        ITopicMessageHandlerConfiguration OnMessage<TMessage, THandler>(string messageType)
            where TMessage : class, new()
            where THandler : class, IMessageHandler<TMessage>;
    }

    internal class MessageHandlerConfiguration : IMessageHandlerConfiguration
    {
        private readonly IServiceCollection _services;
        private readonly MessageHandlerRegistry _handlerRegistry;

        public MessageHandlerConfiguration(IServiceCollection services, MessageHandlerRegistry handlerRegistry)
        {
            _services = services;
            _handlerRegistry = handlerRegistry;
        }

        public ITopicMessageHandlerConfiguration FromTopic(string topic)
        {
            return new TopicMessageHandlerConfiguration(this, topic);
        }

        private class TopicMessageHandlerConfiguration : ITopicMessageHandlerConfiguration
        {
            private readonly MessageHandlerConfiguration _parent;
            private readonly string _topic;

            public TopicMessageHandlerConfiguration(MessageHandlerConfiguration parent, string topic)
            {
                _topic = topic;
                _parent = parent;
            }

            public ITopicMessageHandlerConfiguration OnMessage<TMessage, THandler>(string messageType)
                where TMessage : class, new()
                where THandler : class, IMessageHandler<TMessage>
            {
                _parent._handlerRegistry.Register<TMessage, THandler>(_topic, messageType);
                _parent._services.AddTransient<THandler>();

                return this;
            }
        }
    }
}