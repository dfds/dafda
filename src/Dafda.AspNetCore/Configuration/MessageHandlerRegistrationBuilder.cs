using System;
using Dafda.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    public interface IMessageHandlerRegistrationBuilder
    {
        void AddMessageHandlers(Action<IMessageHandlerTopicBuilder> builder);
    }

    public interface IMessageHandlerTopicBuilder
    {
        IMessageHandlerBuilder FromTopic(string topic);
    }

    public interface IMessageHandlerBuilder
    {
        IMessageHandlerBuilder OnMessage<TMessage, THandler>(string messageType)
            where TMessage : class, new()
            where THandler : class, IMessageHandler<TMessage>;
    }

    internal class MessageHandlerRegistrationBuilder : IMessageHandlerRegistrationBuilder
    {
        private readonly IServiceCollection _services;
        private readonly MessageHandlerRegistry _handlerRegistry;

        public MessageHandlerRegistrationBuilder(IServiceCollection services, MessageHandlerRegistry handlerRegistry)
        {
            _services = services;
            _handlerRegistry = handlerRegistry;
        }

        public void AddMessageHandlers(Action<IMessageHandlerTopicBuilder> builder)
        {
            var handlerConfiguration = new MessageHandlerTopicBuilder(this);

            builder(handlerConfiguration);
        }

        private class MessageHandlerTopicBuilder : IMessageHandlerTopicBuilder
        {
            private readonly MessageHandlerRegistrationBuilder _builder;

            public MessageHandlerTopicBuilder(MessageHandlerRegistrationBuilder builder)
            {
                _builder = builder;
            }

            public IMessageHandlerBuilder FromTopic(string topic)
            {
                return new MessageHandlerBuilder(_builder, topic);
            }
        }

        private class MessageHandlerBuilder : IMessageHandlerBuilder
        {
            private readonly MessageHandlerRegistrationBuilder _parent;
            private readonly string _topic;

            public MessageHandlerBuilder(MessageHandlerRegistrationBuilder parent, string topic)
            {
                _topic = topic;
                _parent = parent;
            }

            public IMessageHandlerBuilder OnMessage<TMessage, THandler>(string messageType)
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