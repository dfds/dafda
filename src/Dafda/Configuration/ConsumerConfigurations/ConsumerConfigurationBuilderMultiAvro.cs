//using Avro.Specific;
//using Dafda.Consuming;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Dafda.Consuming.Avro;
//using Confluent.SchemaRegistry.Serdes;
//using Confluent.SchemaRegistry;
//using Dafda.Consuming.Interfaces;
//using Dafda.Consuming.Factories;
//using Dafda.Consuming.Handlers;
//using Dafda.Consuming.Factories.Schemas.Avro;
//using static System.Net.Mime.MediaTypeNames;

//namespace Dafda.Configuration.ConsumerConfigurations
//{
//    internal class ConsumerConfigurationBuilderMultiAvro
//    {
//        private static readonly string[] DefaultConfigurationKeys =
//        {
//            ConfigurationKey.GroupId,
//            ConfigurationKey.EnableAutoCommit,
//            ConfigurationKey.AllowAutoCreateTopics,
//            ConfigurationKey.BootstrapServers,
//            ConfigurationKey.BrokerVersionFallback,
//            ConfigurationKey.ApiVersionFallbackMs,
//            ConfigurationKey.SslCaLocation,
//            ConfigurationKey.SaslUsername,
//            ConfigurationKey.SaslPassword,
//            ConfigurationKey.SaslMechanisms,
//            ConfigurationKey.SecurityProtocol,
//        };

//        private static readonly string[] RequiredConfigurationKeys =
//        {
//            ConfigurationKey.GroupId,
//            ConfigurationKey.BootstrapServers
//        };

//        private readonly IDictionary<string, string> _configurations = new Dictionary<string, string>();
//        private readonly IList<NamingConvention> _namingConventions = new List<NamingConvention>();

//        private ConfigurationSource _configurationSource = ConfigurationSource.Null;
//        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
//        private Func<IServiceProvider, IIncomingMessageFactory> _incomingMessageFactory = _ => new JsonIncomingMessageFactory();
//        private bool _readFromBeginning;
//        private AvroSerializerConfig _searlizerConfig = null;
//        private SchemaRegistryConfig _schemaRegistryConfig = null;
//        private List<MessageRegistrationBase> _messageRegistrations = new List<MessageRegistrationBase>();
//        private ConsumerErrorHandler _consumerErrorHandler = ConsumerErrorHandler.Default;

//        public ConsumerConfigurationBuilderMultiAvro WithConfigurationSource(ConfigurationSource configurationSource)
//        {
//            _configurationSource = configurationSource;
//            return this;
//        }

//        public ConsumerConfigurationBuilderMultiAvro WithNamingConvention(Func<string, string> converter)
//        {
//            _namingConventions.Add(NamingConvention.UseCustom(converter));
//            return this;
//        }

//        internal ConsumerConfigurationBuilderMultiAvro WithNamingConvention(NamingConvention namingConvention)
//        {
//            _namingConventions.Add(namingConvention);
//            return this;
//        }

//        public ConsumerConfigurationBuilderMultiAvro WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
//        {
//            WithNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));

//            foreach (var additionalPrefix in additionalPrefixes)
//            {
//                WithNamingConvention(NamingConvention.UseEnvironmentStyle(additionalPrefix));
//            }
//            return this;
//        }

//        public ConsumerConfigurationBuilderMultiAvro WithConfiguration(string key, string value)
//        {
//            _configurations[key] = value;
//            return this;
//        }

//        public ConsumerConfigurationBuilderMultiAvro WithGroupId(string groupId)
//        {
//            return WithConfiguration(ConfigurationKey.GroupId, groupId);
//        }

//        public ConsumerConfigurationBuilderMultiAvro WithBootstrapServers(string bootstrapServers)
//        {
//            return WithConfiguration(ConfigurationKey.BootstrapServers, bootstrapServers);
//        }

//        public ConsumerConfigurationBuilderMultiAvro WithUnitOfWorkFactory(IHandlerUnitOfWorkFactory unitOfWorkFactory)
//        {
//            _unitOfWorkFactory = unitOfWorkFactory;
//            return this;
//        }

//        public ConsumerConfigurationBuilderMultiAvro ReadFromBeginning()
//        {
//            _readFromBeginning = true;
//            return this;
//        }

//        public void RegisterMessageResultHandler<TMessage, TMessageHandler, TKey, TValue>(string topic)
//            where TMessageHandler : IMessageHandler<MessageResult<TKey, TValue>> where TMessage : MessageResult<TKey, TValue> where TValue : ISpecificRecord
//        {
//            _messageRegistrations.Add(new MessageRegistration1<TKey, TValue>(topic, typeof(TKey), typeof(TValue), typeof(TMessageHandler)));
//        }

//        public ConsumerConfigurationBuilderMultiAvro RegisterMessageResultHandler<TMessage, TMessageHandler>(string topic)
//            where TMessageHandler : IMessageHandler<MessageResult> where TMessage : MessageResult
//        {
//            //_messageRegistration.Add(new MessageRegistration(topic, typeof(TMessageHandler)));




//            return this;
//        }

//        public ConsumerConfigurationBuilderMultiAvro RegisterMessageHandler<TMessage, TMessageHandler>(string topic)
//            where TMessageHandler : IMessageHandler<TMessage> where TMessage : ISpecificRecord
//        {
//            if (_messageRegistrations != null)
//                throw new Exception("Do real exception here: There is only support for "); //TODO Get back to this

//            //_messageRegistration = new MessageRegistration(topic, typeof(TMessageHandler));
//            return this;
//        }

//        public ConsumerConfigurationBuilderMultiAvro WithAvroSearlizerConfig(AvroSerializerConfig config)
//        {
//            _searlizerConfig = config;
//            return this;
//        }
//        public ConsumerConfigurationBuilderMultiAvro WithSchemaRegistryConfig(SchemaRegistryConfig config)
//        {
//            _schemaRegistryConfig = config;
//            return this;
//        }

//        public ConsumerConfigurationBuilderMultiAvro WithPoisonMessageHandling()
//        {
//            var inner = _incomingMessageFactory;
//            _incomingMessageFactory = provider => new PoisonAwareIncomingMessageFactory(
//                provider.GetRequiredService<ILogger<PoisonAwareIncomingMessageFactory>>(),
//                inner(provider)
//            );
//            return this;
//        }

//        public ConsumerConfigurationBuilderMultiAvro WithConsumerErrorHandler(Func<Exception, Task<ConsumerFailureStrategy>> failureEvaluation)
//        {
//            _consumerErrorHandler = new ConsumerErrorHandler(failureEvaluation);
//            return this;
//        }

//        private List<KafkaAvroBasedConsumerScopeFactoryBase> Test = new List<KafkaAvroBasedConsumerScopeFactoryBase>();

//        internal List<ConsumerConfigurationBase> Build()
//        {
//            Test.Add(new KafkaAvroBasedConsumerScopeFactory<string, string>());
//            foreach (var item in Test)
//            {
//                (KafkaAvroBasedConsumerScopeFactory)item.
//            }
//            var configurations = new ConfigurationBuilder()
//                .WithConfigurationKeys(DefaultConfigurationKeys)
//                .WithRequiredConfigurationKeys(RequiredConfigurationKeys)
//            .WithNamingConventions(_namingConventions.ToArray())
//            .WithConfigurationSource(_configurationSource)
//                .WithConfigurations(_configurations)
//                .Build();

//            if (_searlizerConfig == null)
//                _searlizerConfig = new AvroSerializerConfig();

//            if (_schemaRegistryConfig == null)
//                throw new Exception("Schema registry options not setup"); //TODO: Make this better

//            var consumerConfigs = new List<ConsumerConfigurationBase>();
//            foreach (var messageRegistration in _messageRegistrations)
//            {
//                Func <IServiceProvider, IConsumerScopeFactory<MessageResult>> consumerFactory = provider =>
//                {
//                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

//                    var test = new KafkaAvroBasedConsumerScopeFactory<, >(
//                    loggerFactory: loggerFactory,
//                    configuration: configurations,
//                    topic: messageRegistration.Topic,
//                    readFromBeginning: _readFromBeginning,
//                    schemaRegistryConfig: _schemaRegistryConfig,
//                    avroSerializerConfig: _searlizerConfig
//                    );
//                    return test;
//                };

//                consumerConfigs.Add(new ConsumerConfiguration<,>(
//                configuration: configurations,
//                messageRegistration: _messageRegistrations,
//                unitOfWorkFactory: _unitOfWorkFactory,
//                consumerScopeFactory: consumerFactory,
//                consumerErrorHandler: _consumerErrorHandler
//            ));
//            }

//            return consumerConfigs;




//            //return new ConsumerConfiguration(
//            //    configuration: configurations,
//            //    messageRegistration: _messageRegistrations,
//            //    unitOfWorkFactory: _unitOfWorkFactory,
//            //    consumerScopeFactory: consumerFactory,
//            //    consumerErrorHandler: _consumerErrorHandler
//            //);
//        }
//    }
//}
