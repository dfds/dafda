using Dafda.Consuming;
using Dafda.Consuming.MessageFilters;
using Dafda.Tests.TestDoubles;

namespace Dafda.Tests.Builders
{
    internal class ConsumerBuilder
    {
        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private IConsumerScopeFactory _consumerScopeFactory;
        private MessageHandlerRegistry _registry;
        private IUnconfiguredMessageHandlingStrategy _unconfiguredMessageStrategy;

        private bool _enableAutoCommit;
        private MessageFilter _messageFilter = MessageFilter.Default;

        public ConsumerBuilder()
        {
            _unitOfWorkFactory = new HandlerUnitOfWorkFactoryStub(null);
            _consumerScopeFactory = new ConsumerScopeFactoryStub(new ConsumerScopeStub(new MessageResultBuilder().Build()));
            _registry = new MessageHandlerRegistry();
            _unconfiguredMessageStrategy = new RequireExplicitHandlers();
        }

        public ConsumerBuilder WithUnitOfWork(IHandlerUnitOfWork unitOfWork)
        {
            return WithUnitOfWorkFactory(new HandlerUnitOfWorkFactoryStub(unitOfWork));
        }

        public ConsumerBuilder WithUnitOfWorkFactory(IHandlerUnitOfWorkFactory unitofWorkFactory)
        {
            _unitOfWorkFactory = unitofWorkFactory;
            return this;
        }

        public ConsumerBuilder WithConsumerScopeFactory(IConsumerScopeFactory consumerScopeFactory)
        {
            _consumerScopeFactory = consumerScopeFactory;
            return this;
        }

        public ConsumerBuilder WithMessageHandlerRegistry(MessageHandlerRegistry registry)
        {
            _registry = registry;
            return this;
        }

        public ConsumerBuilder WithEnableAutoCommit(bool enableAutoCommit)
        {
            _enableAutoCommit = enableAutoCommit;
            return this;
        }

        public void WithMessageFilter(MessageFilter messageFilter)
        {
            _messageFilter = messageFilter;
        }

        public ConsumerBuilder WithUnconfiguredMessageStrategy(
            IUnconfiguredMessageHandlingStrategy strategy)
        {
            _unconfiguredMessageStrategy = strategy;
            return this;
        }

        public Consumer Build() =>
            new Consumer(
                _registry,
                _unitOfWorkFactory,
                _consumerScopeFactory,
                _unconfiguredMessageStrategy,
                _messageFilter,
                _enableAutoCommit);
    }
}
