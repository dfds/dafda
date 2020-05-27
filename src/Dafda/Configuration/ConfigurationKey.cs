using System;
using System.Linq;
using System.Reflection;

namespace Dafda.Configuration
{
    internal class ConfigurationKey
    {
        public static readonly ConfigurationKey GroupId = new ConfigurationKey("group.id", ConfigurationKeyGroup.ConsumerOnly);
        public static readonly ConfigurationKey EnableAutoCommit = new ConfigurationKey("enable.auto.commit", ConfigurationKeyGroup.ConsumerOnly);
        public static readonly ConfigurationKey BootstrapServers = new ConfigurationKey("bootstrap.servers");
        public static readonly ConfigurationKey BrokerVersionFallback = new ConfigurationKey("broker.version.fallback");
        public static readonly ConfigurationKey ApiVersionFallbackMs = new ConfigurationKey("api.version.fallback.ms");
        public static readonly ConfigurationKey SslCaLocation = new ConfigurationKey("ssl.ca.location");
        public static readonly ConfigurationKey SaslUsername = new ConfigurationKey("sasl.username");
        public static readonly ConfigurationKey SaslPassword = new ConfigurationKey("sasl.password");
        public static readonly ConfigurationKey SaslMechanisms = new ConfigurationKey("sasl.mechanisms");
        public static readonly ConfigurationKey SecurityProtocol = new ConfigurationKey("security.protocol");

        private static readonly Lazy<ConfigurationKey[]> AllConfigurationKeys = new Lazy<ConfigurationKey[]>(GetAll);

        private static ConfigurationKey[] GetAll()
        {
            var type = typeof(ConfigurationKey);

            return type.GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(field => field.FieldType == type)
                .Select(x => (ConfigurationKey) x.GetValue(null))
                .ToArray();
        }

        private readonly string _key;
        private readonly bool _isConsumer;
        private readonly bool _isProducer;

        private ConfigurationKey(string key, ConfigurationKeyGroup group = ConfigurationKeyGroup.Any)
        {
            _key = key;
            _isConsumer = group == ConfigurationKeyGroup.Any || group == ConfigurationKeyGroup.ConsumerOnly;
            _isProducer = group == ConfigurationKeyGroup.Any || group == ConfigurationKeyGroup.ProducerOnly;
        }

        public static implicit operator string(ConfigurationKey configurationKey)
        {
            return configurationKey.ToString();
        }

        public override string ToString()
        {
            return _key;
        }

        public static ConfigurationKey[] GetAllConsumerKeys()
        {
            return AllConfigurationKeys.Value
                .Where(x => x._isConsumer)
                .ToArray();
        }

        public static ConfigurationKey[] GetAllProducerKeys()
        {
            return AllConfigurationKeys.Value
                .Where(x => x._isProducer)
                .ToArray();
        }

        private enum ConfigurationKeyGroup
        {
            Any,
            ConsumerOnly,
            ProducerOnly
        }
    }
}