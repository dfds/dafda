using System;
using System.Collections.Generic;
using Dafda.Consuming;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.MessageFilters;

namespace Dafda.Configuration;

internal class ConsumerConfiguration(
    IDictionary<string, string> configuration,
    MessageHandlerRegistry messageHandlerRegistry,
    ConsumerConfigurationFactories factories,
    MessageFilter messageFilter,
    IConsumerErrorHandler consumerErrorHandler)
    : ConsumerConfigurationBase(configuration, factories.UnitOfWorkFactory, consumerErrorHandler)
{
    public ConsumerConfigurationFactories Factories { get; } = factories;
    public MessageHandlerRegistry MessageHandlerRegistry { get; } = messageHandlerRegistry;
    public MessageFilter MessageFilter { get; } = messageFilter;
}