using System;
using System.Collections.Generic;
using Dafda.Consuming;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;

namespace Dafda.Configuration;

internal class ConsumerConfiguration(
    IDictionary<string, string> configuration,
    MessageHandlerRegistry messageHandlerRegistry,
    Func<IServiceProvider, IHandlerUnitOfWorkFactory> unitOfWorkFactory,
    Func<IServiceProvider, IUnconfiguredMessageHandlingStrategy> unconfiguredMessageHandlingStrategy,
    Func<IServiceProvider, IConsumerScopeFactory> consumerScopeFactory,
    Func<IServiceProvider, IIncomingMessageFactory> incomingMessageFactory,
    Func<IServiceProvider, IMessageHandlerExecutionStrategy> messageHandlerExecutionStrategyFactory,
    MessageFilter messageFilter,
    IConsumerErrorHandler consumerErrorHandler)
    : ConsumerConfigurationBase(configuration, unitOfWorkFactory, consumerErrorHandler)
{
    public Func<IServiceProvider, IUnconfiguredMessageHandlingStrategy> UnconfiguredMessageHandlingStrategy { get; } = unconfiguredMessageHandlingStrategy;
    public Func<IServiceProvider, IConsumerScopeFactory> ConsumerScopeFactory { get; } = consumerScopeFactory;
    public Func<IServiceProvider, IIncomingMessageFactory> IncomingMessageFactory { get; } = incomingMessageFactory;

    public Func<IServiceProvider, IMessageHandlerExecutionStrategy> MessageHandlerExecutionStrategyFactory { get; } =
        messageHandlerExecutionStrategyFactory;
    public MessageHandlerRegistry MessageHandlerRegistry { get; } = messageHandlerRegistry;
    public MessageFilter MessageFilter { get; } = messageFilter;
}