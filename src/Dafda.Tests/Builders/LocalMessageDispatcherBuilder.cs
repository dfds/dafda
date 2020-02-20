using System;
using Dafda.Consuming;
using Dafda.Tests.TestDoubles;

namespace Dafda.Tests.Builders
{
    internal class LocalMessageDispatcherBuilder
    {
        private IMessageHandlerRegistry _messageHandlerRegistry;
        private IHandlerUnitOfWorkFactory _handlerUnitOfWorkFactory;

        public LocalMessageDispatcherBuilder()
        {
            _messageHandlerRegistry = Dummy.Of<IMessageHandlerRegistry>();
        }

        public LocalMessageDispatcherBuilder WithMessageHandlerRegistry(IMessageHandlerRegistry messageHandlerRegistry)
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