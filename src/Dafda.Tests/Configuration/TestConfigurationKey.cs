using System.Collections.Generic;
using Dafda.Configuration;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConfigurationKey
    {
        [Theory]
        [MemberData(nameof(Data))]
        public void Key_has_correct_name(string expected, string actual)
        {
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new[] {"group.id", ConfigurationKey.GroupId},
            new[] {"enable.auto.commit", ConfigurationKey.EnableAutoCommit},
            new[] {"bootstrap.servers", ConfigurationKey.BootstrapServers},
            new[] {"broker.version.fallback", ConfigurationKey.BrokerVersionFallback},
            new[] {"api.version.fallback.ms", ConfigurationKey.ApiVersionFallbackMs},
            new[] {"ssl.ca.location", ConfigurationKey.SslCaLocation},
            new[] {"sasl.username", ConfigurationKey.SaslUsername},
            new[] {"sasl.password", ConfigurationKey.SaslPassword},
            new[] {"sasl.mechanisms", ConfigurationKey.SaslMechanisms},
            new[] {"security.protocol", ConfigurationKey.SecurityProtocol},
        };

        [Fact]
        public void Has_all_consumer_configuration_keys()
        {
            Assert.Equal(new[]
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
            }, ConfigurationKey.GetAllConsumerKeys());
        }

        [Fact]
        public void Has_all_provider_configuration_keys()
        {
            Assert.Equal(new[]
            {
                ConfigurationKey.BootstrapServers,
                ConfigurationKey.BrokerVersionFallback,
                ConfigurationKey.ApiVersionFallbackMs,
                ConfigurationKey.SslCaLocation,
                ConfigurationKey.SaslUsername,
                ConfigurationKey.SaslPassword,
                ConfigurationKey.SaslMechanisms,
                ConfigurationKey.SecurityProtocol,
            }, ConfigurationKey.GetAllProducerKeys());
        }
    }
}