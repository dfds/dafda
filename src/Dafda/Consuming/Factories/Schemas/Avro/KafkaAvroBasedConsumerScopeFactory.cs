using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Confluent.Kafka.SyncOverAsync;
using System.Linq;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.Avro;
using Dafda.Consuming.Schemas.Avro;

namespace Dafda.Consuming.Factories.Schemas.Avro
{
    internal class KafkaAvroBasedConsumerScopeFactory<TKey, TValue> : IConsumerScopeFactory<MessageResult<TKey, TValue>> where TValue : ISpecificRecord
    {
        internal readonly ILoggerFactory _loggerFactory;
        internal readonly IEnumerable<KeyValuePair<string, string>> _configuration;
        internal readonly string _topic;
        internal readonly bool _readFromBeginning;
        internal readonly SchemaRegistryConfig _schemaRegistryConfig;
        internal readonly AvroSerializerConfig _avroSerializerConfig;

        public KafkaAvroBasedConsumerScopeFactory(ILoggerFactory loggerFactory, IEnumerable<KeyValuePair<string, string>> configuration, string topic, bool readFromBeginning, SchemaRegistryConfig schemaRegistryConfig, AvroSerializerConfig avroSerializerConfig)
        {
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _topic = topic;
            _readFromBeginning = readFromBeginning;
            _schemaRegistryConfig = schemaRegistryConfig;
            _avroSerializerConfig = avroSerializerConfig;
        }

        public IConsumerScope<MessageResult<TKey, TValue>> CreateConsumerScope()
        {
            var schemaRegistry = new CachedSchemaRegistryClient(_schemaRegistryConfig);
            var consumerBuilder = new ConsumerBuilder<TKey, TValue>(_configuration)
                                    .SetKeyDeserializer(new AvroDeserializer<TKey>(schemaRegistry, _avroSerializerConfig).AsSyncOverAsync())
                                    .SetValueDeserializer(new AvroDeserializer<TValue>(schemaRegistry, _avroSerializerConfig).AsSyncOverAsync());

            if (_readFromBeginning)
                consumerBuilder.SetPartitionsAssignedHandler((cons, topicPartitions) => { return topicPartitions.Select(tp => new TopicPartitionOffset(tp, Offset.Beginning)); });

            var consumer = consumerBuilder.Build();
            consumer.Subscribe(_topic);

            return new AvroConsumerScope<TKey, TValue>(_loggerFactory, consumer);
        }
    }
}
