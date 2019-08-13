using System.Collections.Generic;
using System.Linq;
using Dafda.Configuration;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConfigurationBuilder
    {
        [Fact]
        public void Can_validate_configuration()
        {
            var sut = new ConfigurationBuilder();

            Assert.Throws<InvalidConfigurationException>(() => sut.Build());
        }
        
        [Fact]
        public void Can_build_minimal_configuration()
        {
            var configuration = new ConfigurationBuilder()
                .WithGroupId("foo")
                .WithBootstrapServers("bar")
                .Build();

            AssertKeyValue(configuration, "group.id", "foo");
            AssertKeyValue(configuration, "bootstrap.servers", "bar");
        }

        private static void AssertKeyValue(IConfiguration configuration, string expectedKey, string expectedValue)
        {
            configuration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Can_ignore_values_from_configuration_provider()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    ["group.id"] = "foo",
                    ["bootstrap.servers"] = "bar",
                    ["dummy"] = "baz"
                }))
                .Build();

            AssertKeyValue(configuration, "dummy", null);
        }

        [Fact]
        public void Can_use_configuration_value_from_provider()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    ["group.id"] = "foo",
                    ["bootstrap.servers"] = "bar"
                }))
                .Build();

            AssertKeyValue(configuration, "group.id", "foo");
            AssertKeyValue(configuration, "bootstrap.servers", "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_provider_with_environment_naming_convention()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    ["GROUP_ID"] = "foo",
                    ["BOOTSTRAP_SERVERS"] = "bar"
                }))
                .WithEnvironmentNamingConvention()
                .Build();

            AssertKeyValue(configuration, "group.id", "foo");
            AssertKeyValue(configuration, "bootstrap.servers", "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_provider_with_environment_naming_convention_and_prefix()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    ["DEFAULT_KAFKA_GROUP_ID"] = "foo",
                    ["DEFAULT_KAFKA_BOOTSTRAP_SERVERS"] = "bar"
                }))
                .WithEnvironmentNamingConvention("DEFAULT_KAFKA")
                .Build();

            AssertKeyValue(configuration, "group.id", "foo");
            AssertKeyValue(configuration, "bootstrap.servers", "bar");
        }

        [Fact]
        public void Can_overwrite_values_from_provider()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    ["group.id"] = "foo",
                    ["bootstrap.servers"] = "bar"
                }))
                .WithGroupId("baz")
                .Build();

            AssertKeyValue(configuration, "group.id", "baz");
        }

        [Fact]
        public void Only_take_value_from_first_provider_that_matches()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string>
                {
                    ["group.id"] = "foo",
                    ["bootstrap.servers"] = "bar",
                    ["GROUP_ID"] = "baz",
                }))
                .WithNamingConvention(NamingConvention.Default)
                .WithEnvironmentNamingConvention()
                .Build();

            AssertKeyValue(configuration, "group.id", "foo");
        }
    }
}