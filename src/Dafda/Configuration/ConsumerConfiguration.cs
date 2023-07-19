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
            IConsumerErrorHandler consumerErrorHandler) : base(configuration, unitOfWorkFactory, consumerErrorHandler)
        {
            MessageHandlerRegistry = messageHandlerRegistry;
            ConsumerScopeFactory = consumerScopeFactory;
            MessageFilter = messageFilter;
            IncomingMessageFactory = incomingMessageFactory;
        }

        public Func<IServiceProvider, IConsumerScopeFactory> ConsumerScopeFactory { get; }
        public Func<IServiceProvider, IIncomingMessageFactory> IncomingMessageFactory { get; }
        public MessageHandlerRegistry MessageHandlerRegistry { get; }
        public MessageFilter MessageFilter { get; }
    }
}