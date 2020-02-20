using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Consuming;
using Dafda.Logging;

namespace Dafda.Configuration
{
    public sealed class ConsumerConfigurationBuilder
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
        private IConsumerScopeFactory _consumerScopeFactory;

        internal ConsumerConfigurationBuilder()
        {
        }

        public void WithConfigurationSource(ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
        }

        public void WithNamingConvention(NamingConvention namingConvention)
        {
            _namingConventions.Add(namingConvention);
        }

        public void WithEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            WithNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));

            foreach (var additionalPrefix in additionalPrefixes)
            {
                WithNamingConvention(NamingConvention.UseEnvironmentStyle(additionalPrefix));
            }
        }

        public void WithConfiguration(string key, string value)
        {
            _configurations[key] = value;
        }

        public void WithGroupId(string groupId)
        {
            WithConfiguration(ConfigurationKey.GroupId, groupId);
        }

        public void WithBootstrapServers(string bootstrapServers)
        {
            WithConfiguration(ConfigurationKey.BootstrapServers, bootstrapServers);
        }

        public void WithUnitOfWorkFactory(IHandlerUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public void WithUnitOfWorkFactory(Func<Type, IHandlerUnitOfWork> factory)
        {
            _unitOfWorkFactory = new DefaultUnitOfWorkFactory(factory);
        }
        
        public void WithConsumerScopeFactory(IConsumerScopeFactory consumerScopeFactory)
        {
            _consumerScopeFactory = consumerScopeFactory;
        }

        public void RegisterMessageHandler<TMessage, TMessageHandler>(string topic, string messageType)
            where TMessage : class, new()
            where TMessageHandler : IMessageHandler<TMessage>
        {
            _messageHandlerRegistry.Register<TMessage, TMessageHandler>(topic, messageType);
        }

        internal ConsumerConfiguration Build()
        {
            if (!_namingConventions.Any())
            {
                _namingConventions.Add(item: NamingConvention.Default);
            }

            FillConfiguration();
            ValidateConfiguration();

            if (_consumerScopeFactory == null)
            {
                _consumerScopeFactory = new KafkaBasedConsumerScopeFactory(
                    configuration: _configurations,
                    topics: _messageHandlerRegistry.GetAllSubscribedTopics()
                );
            }
            
            return new ConsumerConfiguration(
                configuration: _configurations, 
                messageHandlerRegistry: _messageHandlerRegistry, 
                unitOfWorkFactory: _unitOfWorkFactory, 
                consumerScopeFactory: _consumerScopeFactory
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

    }
}