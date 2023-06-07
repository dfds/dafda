using Avro.Specific;
using Dafda.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dafda.Consuming.Avro;
using Confluent.SchemaRegistry.Serdes;
using Confluent.SchemaRegistry;
using Dafda.Consuming.Interfaces;
using Dafda.Consuming.Factories;
using Dafda.Consuming.Handlers;
using Dafda.Consuming.Factories.Schemas.Avro;

namespace Dafda.Configuration.ConsumerConfigurations
{
    internal sealed class ConsumerConfigurationBuilderAvro<TKey, TValue> where TValue : ISpecificRecord
    {
        private static readonly string[] DefaultConfigurationKeys =
        {
            ConfigurationKey.GroupId,
            ConfigurationKey.EnableAutoCommit,
            ConfigurationKey.AllowAutoCreateTopics,
            ConfigurationKey.BootstrapServers,
            ConfigurationKey.BrokerVersionFallback,
            ConfigurationKey.ApiVersionFallbackMs,
            ConfigurationKey.SslCaLocation,
            ConfigurationKey.SaslUsername,
            ConfigurationKey.SaslPassword,
            ConfigurationKey.SaslMechanisms,
            ConfigurationKey.SecurityProtocol,
        };

        private static readonly string[] RequiredConfigurationKeys =
        {
            ConfigurationKey.GroupId,
            ConfigurationKey.BootstrapServers
        };

        private readonly IDictionary<string, string> _configurations = new Dictionary<string, string>();
        private readonly IList<NamingConvention> _namingConventions = new List<NamingConvention>();

        private ConfigurationSource _configurationSource = ConfigurationSource.Null;
        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private Func<IServiceProvider, IIncomingMessageFactory> _incomingMessageFactory = _ => new JsonIncomingMessageFactory();
        private bool _readFromBeginning;
        private AvroSerializerConfig _searlizerConfig = null;
        private SchemaRegistryConfig _schemaRegistryConfig = null;
        private MessageRegistration<TKey, TValue> _messageRegistration = null;
        private ConsumerErrorHandler _consumerErrorHandler = ConsumerErrorHandler.Default;

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithNamingConvention(Func<string, string> converter)
        {
            _namingConventions.Add(NamingConvention.UseCustom(converter));
            return this;
        }

        internal ConsumerConfigurationBuilderAvro<TKey, TValue> WithNamingConvention(NamingConvention namingConvention)
        {
            _namingConventions.Add(namingConvention);
            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            WithNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));

            foreach (var additionalPrefix in additionalPrefixes)
            {
                WithNamingConvention(NamingConvention.UseEnvironmentStyle(additionalPrefix));
            }

            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithConfiguration(string key, string value)
        {
            _configurations[key] = value;
            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithGroupId(string groupId)
        {
            return WithConfiguration(ConfigurationKey.GroupId, groupId);
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithBootstrapServers(string bootstrapServers)
        {
            return WithConfiguration(ConfigurationKey.BootstrapServers, bootstrapServers);
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithUnitOfWorkFactory(IHandlerUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> ReadFromBeginning()
        {
            _readFromBeginning = true;
            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> RegisterMessageResultHandler<TMessage, TMessageHandler>(string topic)
            where TMessageHandler : IMessageHandler<MessageResult<TKey, TValue>> where TMessage : MessageResult<TKey, TValue>
        {
            if (_messageRegistration != null)
                throw new Exception("Do real exception here: There is only support for "); //TODO Get back to this

            _messageRegistration = new MessageRegistration<TKey, TValue>(topic, typeof(TMessageHandler));
            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> RegisterMessageHandler<TMessage, TMessageHandler>(string topic)
            where TMessageHandler : IMessageHandler<TMessage> where TMessage : ISpecificRecord
        {
            if (_messageRegistration != null)
                throw new Exception("Do real exception here: There is only support for "); //TODO Get back to this

            _messageRegistration = new MessageRegistration<TKey, TValue>(topic, typeof(TMessageHandler));
            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithAvroSearlizerConfig(AvroSerializerConfig config)
        {
            _searlizerConfig = config;
            return this;
        }
        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithSchemaRegistryConfig(SchemaRegistryConfig config)
        {
            _schemaRegistryConfig = config;
            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithPoisonMessageHandling()
        {
            var inner = _incomingMessageFactory;
            _incomingMessageFactory = provider => new PoisonAwareIncomingMessageFactory(
                provider.GetRequiredService<ILogger<PoisonAwareIncomingMessageFactory>>(),
                inner(provider)
            );
            return this;
        }

        public ConsumerConfigurationBuilderAvro<TKey, TValue> WithConsumerErrorHandler(Func<Exception, Task<ConsumerFailureStrategy>> failureEvaluation)
        {
            _consumerErrorHandler = new ConsumerErrorHandler(failureEvaluation);
            return this;
        }

        internal ConsumerConfiguration<TKey, TValue> Build()
        {
            var configurations = new ConfigurationBuilder()
                .WithConfigurationKeys(DefaultConfigurationKeys)
                .WithRequiredConfigurationKeys(RequiredConfigurationKeys)
                .WithNamingConventions(_namingConventions.ToArray())
                .WithConfigurationSource(_configurationSource)
                .WithConfigurations(_configurations)
                .Build();

            if (_searlizerConfig == null)
                _searlizerConfig = new AvroSerializerConfig();

            if (_schemaRegistryConfig == null)
                throw new Exception("Schema registry options not setup"); //TODO: Make this better

            Func<IServiceProvider, IConsumerScopeFactory<MessageResult<TKey, TValue>>> consumerFactory = provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

            return new KafkaAvroBasedConsumerScopeFactory<TKey, TValue>(
                loggerFactory: loggerFactory,
                configuration: configurations,
                topic: _messageRegistration.Topic,
                readFromBeginning: _readFromBeginning,
                schemaRegistryConfig: _schemaRegistryConfig,
                avroSerializerConfig: _searlizerConfig
                );
        };

            return new ConsumerConfiguration<TKey, TValue>(
                configuration: configurations,
                messageRegistration: _messageRegistration,
                unitOfWorkFactory: _unitOfWorkFactory,
                consumerScopeFactory: consumerFactory,
                consumerErrorHandler: _consumerErrorHandler
            );
        }

    }
}
