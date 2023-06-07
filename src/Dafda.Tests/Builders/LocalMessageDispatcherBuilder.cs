using Dafda.Consuming;
using Dafda.Consuming.Handlers;
using Dafda.Consuming.Interfaces;
using Dafda.Tests.TestDoubles;

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

        public LocalMessageDispatcherBuilder WithHandlerUnitOfWork(IHandlerUnitOfWork unitOfWork)
        {
            return WithHandlerUnitOfWorkFactory(new HandlerUnitOfWorkFactoryStub(unitOfWork));
        }

        public LocalMessageDispatcher Build()
        {
            return new LocalMessageDispatcher(
                _messageHandlerRegistry,
                _handlerUnitOfWorkFactory,
                new RequireExplicitHandlers());
        }
    }
}
