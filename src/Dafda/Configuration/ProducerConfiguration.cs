using System;
using System.Collections.Generic;
using Dafda.Producing;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration;

internal class ProducerConfiguration(
    IDictionary<string, string> configuration,
    MessageIdGenerator messageIdGenerator,
    Func<IServiceProvider, KafkaProducer> kafkaProducerFactory)
{
    public IDictionary<string, string> KafkaConfiguration { get; } = configuration;
    public MessageIdGenerator MessageIdGenerator { get; } = messageIdGenerator;
    public Func<IServiceProvider, KafkaProducer> KafkaProducerFactory { get; } = kafkaProducerFactory;
}