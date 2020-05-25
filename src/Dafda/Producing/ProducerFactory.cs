using System;
using System.Collections.Generic;
using Dafda.Configuration;
using Microsoft.Extensions.Logging;

namespace Dafda.Producing
{
    internal sealed class ProducerFactory : IDisposable
    {
        private readonly Dictionary<string, ProducerBuilder> _producerBuilders = new Dictionary<string, ProducerBuilder>();

        internal void ConfigureProducer(string producerName, ProducerConfiguration configuration, OutgoingMessageRegistry outgoingMessageRegistry)
        {
            if (_producerBuilders.ContainsKey(producerName))
            {
                throw new ProducerFactoryException($"A producer with the name \"{producerName}\" has already been configured. Producer names should be unique.");
            }

            _producerBuilders.Add(producerName, new ProducerBuilder(
                    producerName: producerName,
                    configuration: configuration, 
                    messageRegistry: outgoingMessageRegistry
                ));
        }

        internal static string GetKeyNameOf<TClient>() => $"__INTERNAL__FOR_CLIENT__{typeof(TClient).FullName}";

        internal void ConfigureProducerFor<TClient>(ProducerConfiguration configuration, OutgoingMessageRegistry outgoingMessageRegistry)
        {
            var producerName = GetKeyNameOf<TClient>();
            ConfigureProducer(producerName, configuration, outgoingMessageRegistry);
        }

        public Producer Get(string producerName, ILoggerFactory loggerFactory) 
        {
            if (_producerBuilders.TryGetValue(producerName, out var builder))
            {
                return builder.Build(loggerFactory);
            }

            return null;
        }

        public Producer GetFor<TClient>(ILoggerFactory loggerFactory)
        {
            var producerName = GetKeyNameOf<TClient>();
            return Get(producerName, loggerFactory);
        }

        public void Dispose()
        {
            if (_producerBuilders != null)
            {
                foreach (var registration in _producerBuilders.Values)
                {
                    registration.Dispose();
                }
            }
        }

        private class ProducerBuilder : IDisposable
        {
            private readonly OutgoingMessageRegistry _messageRegistry;
            private readonly Func<ILoggerFactory, KafkaProducer> _kafkaProducerFactory;
            private readonly MessageIdGenerator _messageIdGenerator;
            private readonly string _producerName;
            private KafkaProducer _kafkaProducer;

            public ProducerBuilder(string producerName, ProducerConfiguration configuration, OutgoingMessageRegistry messageRegistry)
            {
                _messageRegistry = messageRegistry;
                _producerName = producerName;
                _kafkaProducerFactory = configuration.KafkaProducerFactory;
                _messageIdGenerator = configuration.MessageIdGenerator;
            }

            public Producer Build(ILoggerFactory loggerFactory)
            {
                if (_kafkaProducer is null)
                {
                    _kafkaProducer = _kafkaProducerFactory(loggerFactory);
                }

                var producer = new Producer(
                    kafkaProducer: _kafkaProducer,
                    outgoingMessageRegistry: _messageRegistry,
                    messageIdGenerator: _messageIdGenerator
                );

                producer.Name = _producerName;

                return producer;
            }

            public void Dispose()
            {
                _kafkaProducer?.Dispose();
            }
        }
    }
}