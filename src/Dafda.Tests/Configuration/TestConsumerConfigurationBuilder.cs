using System.Collections.Generic;
using System.Linq;
using Dafda.Configuration;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConsumerConfigurationBuilder
    {
        [Fact]
        public void Can_validate_configuration()
        {
            var sut = new ConsumerConfigurationBuilder();

            Assert.Throws<InvalidConfigurationException>(() => sut.Build());
        }
        
        [Fact]
        public void Can_build_minimal_configuration()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithConfiguration(ConfigurationProperties.GroupId, "foo")
                .WithConfiguration(ConfigurationProperties.BootstrapServers, "bar")
                .Build();

            AssertKeyValue(configuration, ConfigurationProperties.GroupId, "foo");
            AssertKeyValue(configuration, ConfigurationProperties.BootstrapServers, "bar");
        }

        private static void AssertKeyValue(IConfiguration configuration, string expectedKey, string expectedValue)
        {
            configuration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Can_ignore_values_from_configuration_provider()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    [ConfigurationProperties.GroupId] = "foo",
                    [ConfigurationProperties.BootstrapServers] = "bar",
                    ["dummy"] = "baz"
                }))
                .Build();

            AssertKeyValue(configuration, "dummy", null);
        }

        [Fact]
        public void Can_use_configuration_value_from_provider()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    [ConfigurationProperties.GroupId] = "foo",
                    [ConfigurationProperties.BootstrapServers] = "bar"
                }))
                .Build();

            AssertKeyValue(configuration, ConfigurationProperties.GroupId, "foo");
            AssertKeyValue(configuration, ConfigurationProperties.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_provider_with_environment_naming_convention()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    ["GROUP_ID"] = "foo",
                    ["BOOTSTRAP_SERVERS"] = "bar"
                }))
                .WithEnvironmentNamingConvention()
                .Build();

            AssertKeyValue(configuration, ConfigurationProperties.GroupId, "foo");
            AssertKeyValue(configuration, ConfigurationProperties.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_provider_with_environment_naming_convention_and_prefix()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    ["DEFAULT_KAFKA_GROUP_ID"] = "foo",
                    ["DEFAULT_KAFKA_BOOTSTRAP_SERVERS"] = "bar"
                }))
                .WithEnvironmentNamingConvention("DEFAULT_KAFKA")
                .Build();

            AssertKeyValue(configuration, ConfigurationProperties.GroupId, "foo");
            AssertKeyValue(configuration, ConfigurationProperties.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_overwrite_values_from_provider()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    [ConfigurationProperties.GroupId] = "foo",
                    [ConfigurationProperties.BootstrapServers] = "bar"
                })).WithConfiguration(ConfigurationProperties.GroupId, "baz")
                .Build();

            AssertKeyValue(configuration, ConfigurationProperties.GroupId, "baz");
        }

        [Fact]
        public void Only_take_value_from_first_provider_that_matches()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    [ConfigurationProperties.GroupId] = "foo",
                    [ConfigurationProperties.BootstrapServers] = "bar",
                    ["GROUP_ID"] = "baz",
                }))
                .WithNamingConvention(NamingConvention.Default)
                .WithEnvironmentNamingConvention()
                .Build();

            AssertKeyValue(configuration, ConfigurationProperties.GroupId, "foo");
        }
    }
}