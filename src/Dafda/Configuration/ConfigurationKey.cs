using System;
using System.Linq;
using System.Reflection;

namespace Dafda.Configuration
{
    /// <summary>
    /// Configuration keys
    /// </summary>
    public class ConfigurationKey
    {
        /// <summary> Config key for group.id</summary>
        public static readonly ConfigurationKey GroupId = new ConfigurationKey("group.id", ConfigurationKeyGroup.ConsumerOnly);

        /// <summary> Config key for enable.auto.commit</summary>
        public static readonly ConfigurationKey EnableAutoCommit = new ConfigurationKey("enable.auto.commit", ConfigurationKeyGroup.ConsumerOnly);

        /// <summary> Config key for allow.auto.create.topics</summary>
        public static readonly ConfigurationKey AllowAutoCreateTopics = new ConfigurationKey("allow.auto.create.topics", ConfigurationKeyGroup.ConsumerOnly);

        /// <summary> Config key for bootstrap.servers</summary>
        public static readonly ConfigurationKey BootstrapServers = new ConfigurationKey("bootstrap.servers");

        /// <summary> Config key for broker.version.fallback</summary>
        public static readonly ConfigurationKey BrokerVersionFallback = new ConfigurationKey("broker.version.fallback");

        /// <summary> Config key for api.version.fallback.ms</summary>
        public static readonly ConfigurationKey ApiVersionFallbackMs = new ConfigurationKey("api.version.fallback.ms");

        /// <summary> Config key for ssl.ca.location</summary>
        public static readonly ConfigurationKey SslCaLocation = new ConfigurationKey("ssl.ca.location");

        /// <summary> Config key for sasl.username</summary>
        public static readonly ConfigurationKey SaslUsername = new ConfigurationKey("sasl.username");

        /// <summary> Config key for sasl.password</summary>
        public static readonly ConfigurationKey SaslPassword = new ConfigurationKey("sasl.password");

        /// <summary> Config key for sasl.mechanisms</summary>
        public static readonly ConfigurationKey SaslMechanisms = new ConfigurationKey("sasl.mechanisms");

        /// <summary> Config key for security.protocol</summary>
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

        /// <summary>Override implecit operator to string</summary>
        public static implicit operator string(ConfigurationKey configurationKey)
        {
            return configurationKey.ToString();
        }

        /// <summary>Override ToString</summary>
        public override string ToString()
        {
            return _key;
        }

        /// <summary>Get all keys related to consumers</summary>
        public static ConfigurationKey[] GetAllConsumerKeys()
        {
            return AllConfigurationKeys.Value
                .Where(x => x._isConsumer)
                .ToArray();
        }

        /// <summary>Get all keys related to producers</summary>
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