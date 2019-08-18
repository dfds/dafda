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

        public ConsumerConfigurationBuilder UseConfigurationSource(ConfigurationSource configurationSource)
        {
            SetConfigurationSource(configurationSource);
            return this;
        }

        public ConsumerConfigurationBuilder AppendNamingConvention(NamingConvention namingConvention)
        {
            AddNamingConvention(namingConvention);
            return this;
        }

        public ConsumerConfigurationBuilder AppendEnvironmentStyle(string prefix = null, params string[] additionalPrefixes)
        {
            AppendNamingConvention(NamingConvention.UseEnvironmentStyle(prefix));

            foreach (var additionalPrefix in additionalPrefixes)
            {
                AppendNamingConvention(NamingConvention.UseEnvironmentStyle(additionalPrefix));
            }

            return this;
        }

        public ConsumerConfigurationBuilder WithConfiguration(string key, string value)
        {
            SetConfigurationValue(key, value);
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