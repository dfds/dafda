using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dafda.Consuming;
using Dafda.Logging;
using Dafda.Messaging;

namespace Dafda.Configuration
{
    public class ConsumerConfigurationBuilder
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private static readonly string[] DefaultConfigurationKeys =
        {
            ConfigurationKey.GroupId,
            ConfigurationKey.EnableAutoCommit,
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
        private readonly IMessageHandlerRegistry _messageHandlerRegistry = new MessageHandlerRegistry();

        private ConfigurationSource _configurationSource = ConfigurationSource.Null;
        private IHandlerUnitOfWorkFactory _unitOfWorkFactory;
        private IInternalConsumerFactory _internalConsumerFactory = new KafkaBasedConsumerFactory();

        public ConsumerConfigurationBuilder WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
            return this;
        }

        public ConsumerConfigurationBuilder WithNamingConvention(NamingConvention namingConvention)
        {
            _namingConventions.Add(namingConvention);
            return this;
        }

        public ConsumerConfigurationBuilder WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            WithNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));

            foreach (var additionalPrefix in additionalPrefixes)
            {
                WithNamingConvention(NamingConvention.UseEnvironmentStyle(additionalPrefix));
            }

            return this;
        }

        public ConsumerConfigurationBuilder WithConfiguration(string key, string value)
        {
            _configurations[key] = value;
            return this;
        }

        public ConsumerConfigurationBuilder WithGroupId(string groupId)
        {
            return WithConfiguration(ConfigurationKey.GroupId, groupId);
        }

        public ConsumerConfigurationBuilder WithBootstrapServers(string bootstrapServers)
        {
            return WithConfiguration(ConfigurationKey.BootstrapServers, bootstrapServers);
        }

        public ConsumerConfigurationBuilder WithUnitOfWorkFactory(IHandlerUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            return this;
        }

        public ConsumerConfigurationBuilder WithUnitOfWorkFactory(Func<Type, IHandlerUnitOfWork> factory)
        {
            _unitOfWorkFactory = new DefaultUnitOfWorkFactory(factory);
            return this;
        }
        
        public ConsumerConfigurationBuilder WithInternalConsumerFactory(IInternalConsumerFactory internalConsumerFactory)
        {
            _internalConsumerFactory = internalConsumerFactory;
            return this;
        }

        public ConsumerConfigurationBuilder RegisterMessageHandler<TMessage, TMessageHandler>(string topic, string messageType)
            where TMessage : class, new()
            where TMessageHandler : IMessageHandler<TMessage>
        {
            _messageHandlerRegistry.Register<TMessage, TMessageHandler>(topic, messageType);
            return this;
        }

        public IConsumerConfiguration Build()
        {
            if (!_namingConventions.Any())
            {
                _namingConventions.Add(item: NamingConvention.Default);
            }

            FillConfiguration();
            ValidateConfiguration();

            return new ConsumerConfiguration(
                configuration: _configurations, 
                messageHandlerRegistry: _messageHandlerRegistry, 
                unitOfWorkFactory: _unitOfWorkFactory, 
                internalConsumerFactory: _internalConsumerFactory
            );
        }

        private void FillConfiguration()
        {
            foreach (var key in AllKeys)
            {
                if (_configurations.ContainsKey(key))
                {
                    continue;
                }

                var value = GetByKey(key);
                if (value != null)
                {
                    _configurations[key] = value;
                }
            }
        }

        private static IEnumerable<string> AllKeys => DefaultConfigurationKeys.Concat(RequiredConfigurationKeys).Distinct();

        private string GetByKey(string key)
        {
            Logger.Debug("Looking for {Key} in {SourceName} using keys {AttemptedKeys}", key, GetSourceName(), GetAttemptedKeys(key));

            return _namingConventions
                .Select(namingConvention => namingConvention.GetKey(key))
                .Select(actualKey => _configurationSource.GetByKey(actualKey))
                .FirstOrDefault(value => value != null);
        }

        private string GetSourceName()
        {
            return _configurationSource.GetType().Name;
        }

        private IEnumerable<string> GetAttemptedKeys(string key)
        {
            return _namingConventions.Select(convention => convention.GetKey(key));
        }

        private void ValidateConfiguration()
        {
            foreach (var key in RequiredConfigurationKeys)
            {
                if (!_configurations.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
                {
                    var message = $"Expected key '{key}' not supplied in '{GetSourceName()}' (attempted keys: '{string.Join("', '", GetAttemptedKeys(key))}')";
                    throw new InvalidConfigurationException(message);
                }
            }
        }

        private class ConsumerConfiguration : IConsumerConfiguration
        {
            private readonly IDictionary<string, string> _configuration;

            public ConsumerConfiguration(IDictionary<string, string> configuration, IMessageHandlerRegistry messageHandlerRegistry, 
                IHandlerUnitOfWorkFactory unitOfWorkFactory, IInternalConsumerFactory internalConsumerFactory)
            {
                _configuration = configuration;
                MessageHandlerRegistry = messageHandlerRegistry;
                UnitOfWorkFactory = unitOfWorkFactory;
                InternalConsumerFactory = internalConsumerFactory;
            }

            public IMessageHandlerRegistry MessageHandlerRegistry { get; }
            public IHandlerUnitOfWorkFactory UnitOfWorkFactory { get; }
            public IInternalConsumerFactory InternalConsumerFactory { get; }

            public bool EnableAutoCommit
            {
                get
                {
                    const bool defaultAutoCommitStrategy = true;
                    
                    if (!_configuration.TryGetValue(ConfigurationKey.EnableAutoCommit, out var value))
                    {
                        return defaultAutoCommitStrategy;
                    }

                    return bool.Parse(value);
                }
            }

            public IEnumerable<string> SubscribedTopics => MessageHandlerRegistry.GetAllSubscribedTopics(); 

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return _configuration.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable) _configuration).GetEnumerator();
            }
        }
    }
}