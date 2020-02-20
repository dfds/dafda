using System;
using Dafda.Consuming;
using Dafda.Tests.TestDoubles;

namespace Dafda.Tests.Builders
{
    internal class ConsumerBuilder
    {
        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private IConsumerScopeFactory _consumerScopeFactory;
        private MessageHandlerRegistry _registry;

        private bool _enableAutoCommit;

        public ConsumerBuilder()
        {
            _unitOfWorkFactory = new HandlerUnitOfWorkFactoryStub(null);

            var messageStub = new MessageResultBuilder().Build();
            _consumerScopeFactory = new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageStub));
            _registry = new MessageHandlerRegistry();
        }

        public ConsumerBuilder WithUnitOfWorkFactory(Func<Type, IHandlerUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = new DefaultUnitOfWorkFactory(unitOfWorkFactory);
            return this;
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

        public Consumer Build()
        {
            return new Consumer(
                messageHandlerRegistry: _registry,
                unitOfWorkFactory: _unitOfWorkFactory,
                consumerScopeFactory: _consumerScopeFactory,
                isAutoCommitEnabled: _enableAutoCommit
            );
        }
    }
}