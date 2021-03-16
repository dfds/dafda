using System;
using Dafda.Consuming;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dafda.Tests.Builders
{
    internal class ConsumerBuilder
    {
        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private Func<ILoggerFactory, IConsumerScopeFactory> _consumerScopeFactory;
        private MessageHandlerRegistry _registry;
        private IUnconfiguredMessageHandlingStrategy _unconfiguredMessageStrategy;

        private bool _enableAutoCommit;

        public ConsumerBuilder()
        {
            _unitOfWorkFactory = new HandlerUnitOfWorkFactoryStub(null);
            _consumerScopeFactory =
                _ =>
                    new ConsumerScopeFactoryStub(
                        new ConsumerScopeStub(
                            new MessageResultBuilder().Build()));
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

        public ConsumerBuilder WithConsumerScopeFactory(
            Func<ILoggerFactory, IConsumerScopeFactory> consumerScopeFactory)
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
                _consumerScopeFactory(NullLoggerFactory.Instance),
                _unconfiguredMessageStrategy,
                _enableAutoCommit);
    }
}
