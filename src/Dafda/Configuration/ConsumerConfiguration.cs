using System;
using System.Collections.Generic;
using Dafda.Consuming;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;

namespace Dafda.Configuration
{
    internal class ConsumerConfiguration : ConsumerConfigurationBase
    {
        public ConsumerConfiguration(IDictionary<string, string> configuration,
            MessageHandlerRegistry messageHandlerRegistry,
            IHandlerUnitOfWorkFactory unitOfWorkFactory,
            Func<IServiceProvider, IConsumerScopeFactory> consumerScopeFactory,
            Func<IServiceProvider, IIncomingMessageFactory> incomingMessageFactory,
            MessageFilter messageFilter,
            IConsumerErrorHandler consumerErrorHandler,
            Func<IServiceProvider, IConsumerExecutionStrategy> executionStrategyFactory) : base(configuration, unitOfWorkFactory, consumerErrorHandler)
        {
            MessageHandlerRegistry = messageHandlerRegistry;
            ConsumerScopeFactory = consumerScopeFactory;
            MessageFilter = messageFilter;
            IncomingMessageFactory = incomingMessageFactory;
            ExecutionStrategyFactory = executionStrategyFactory;
        }

        public Func<IServiceProvider, IConsumerScopeFactory> ConsumerScopeFactory { get; }
        public Func<IServiceProvider, IIncomingMessageFactory> IncomingMessageFactory { get; }
        public Func<IServiceProvider, IConsumerExecutionStrategy> ExecutionStrategyFactory { get; }
        public MessageHandlerRegistry MessageHandlerRegistry { get; }
        public MessageFilter MessageFilter { get; }
    }
}