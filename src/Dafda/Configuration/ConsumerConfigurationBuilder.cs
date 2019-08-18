using System.Collections.Generic;

namespace Dafda.Configuration
{
    public class ConsumerConfigurationBuilder : ConfigurationBuilderBase
    {
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

        public ConsumerConfigurationBuilder WithConfigurationSource(ConfigurationSource configurationSource)
        {
            SetConfigurationSource(configurationSource);
            return this;
        }

        public ConsumerConfigurationBuilder WithNamingConvention(NamingConvention namingConvention)
        {
            AddNamingConvention(namingConvention);
            return this;
        }

        public ConsumerConfigurationBuilder WithConfiguration(string key, string value)
        {
            SetConfigurationValue(key, value);
            return this;
        }

        protected override IEnumerable<string> GetRequiredConfigurationKeys()
        {
            return RequiredConfigurationKeys;
        }

        protected override IEnumerable<string> GetDefaultConfigurationKeys()
        {
            return DefaultConfigurationKeys;
        }
    }
}