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

        private bool _enableAutoCommit;

        public ConsumerBuilder()
        {
            _unitOfWorkFactory = new HandlerUnitOfWorkFactoryStub(null);

            var messageStub = new MessageResultBuilder().Build();
            _consumerScopeFactory = _ => new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageStub));
            _registry = new MessageHandlerRegistry();
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

        public ConsumerBuilder WithConsumerScopeFactory(Func<ILoggerFactory, IConsumerScopeFactory> consumerScopeFactory)
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
                consumerScopeFactory: _consumerScopeFactory(NullLoggerFactory.Instance),
                isAutoCommitEnabled: _enableAutoCommit
            );
        }
    }
}