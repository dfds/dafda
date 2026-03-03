using System;
using Dafda.Consuming;

namespace Dafda.Configuration;

internal sealed record ConsumerConfigurationFactories(
    Func<IServiceProvider, IHandlerUnitOfWorkFactory> UnitOfWorkFactory,
    Func<IServiceProvider, IUnconfiguredMessageHandlingStrategy> UnconfiguredMessageHandlingStrategy,
    Func<IServiceProvider, IConsumerScopeFactory> ConsumerScopeFactory,
    Func<IServiceProvider, IIncomingMessageFactory> IncomingMessageFactory,
    Func<IServiceProvider, IMessageHandlerExecutionStrategy> MessageHandlerExecutionStrategyFactory);
