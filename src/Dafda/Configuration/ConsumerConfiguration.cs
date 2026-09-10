namespace Dafda.Configuration;

using System;
using System.Collections.Generic;
using Consuming;
using Consuming.Interfaces;
using Consuming.MessageFilters;

internal class ConsumerConfiguration(
    IDictionary<string, string> configuration,
    MessageHandlerRegistry messageHandlerRegistry,
    ConsumerConfigurationFactories factories,
    MessageFilter messageFilter,
    IConsumerErrorHandler consumerErrorHandler,
    Func<IServiceProvider, IDeadLetterQueue> deadLetterQueueFactory,
    int maxRetries,
    Func<Exception, bool> deadLetterQueueBypass)
    : ConsumerConfigurationBase(configuration, factories.UnitOfWorkFactory, consumerErrorHandler)
{
    public ConsumerConfigurationFactories Factories { get; } = factories;
    public MessageHandlerRegistry MessageHandlerRegistry { get; } = messageHandlerRegistry;
    public MessageFilter MessageFilter { get; } = messageFilter;
    public Func<IServiceProvider, IDeadLetterQueue> DeadLetterQueueFactory { get; } = deadLetterQueueFactory;
    public int MaxRetries { get; } = maxRetries;
    public Func<Exception, bool> DeadLetterQueueBypass { get; } = deadLetterQueueBypass;
}