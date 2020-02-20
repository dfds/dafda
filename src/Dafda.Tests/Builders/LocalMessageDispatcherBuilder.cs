using System;
using Dafda.Consuming;

namespace Dafda.Tests.Builders
{
    internal class LocalMessageDispatcherBuilder
    {
        private MessageHandlerRegistry _messageHandlerRegistry;
        private IHandlerUnitOfWorkFactory _handlerUnitOfWorkFactory;

        public LocalMessageDispatcherBuilder()
        {
            _messageHandlerRegistry = new MessageHandlerRegistry();
        }

        public LocalMessageDispatcherBuilder WithMessageHandlerRegistry(MessageHandlerRegistry messageHandlerRegistry)
        {
            _messageHandlerRegistry = messageHandlerRegistry;
            return this;
        }

        public LocalMessageDispatcherBuilder WithHandlerUnitOfWorkFactory(IHandlerUnitOfWorkFactory handlerUnitOfWorkFactory)
        {
            _handlerUnitOfWorkFactory = handlerUnitOfWorkFactory;
            return this;
        }

        public LocalMessageDispatcherBuilder WithHandlerUnitOfWorkFactory(Func<Type, IHandlerUnitOfWork> factory)
        {
            _handlerUnitOfWorkFactory = new DefaultUnitOfWorkFactory(factory);
            return this;
        }

        public LocalMessageDispatcher Build()
        {
            return new LocalMessageDispatcher(_messageHandlerRegistry, _handlerUnitOfWorkFactory);
        }
    }
}