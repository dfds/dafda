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
        public void Can_build_empty_configuration()
        {
            var configuration = new ConfigurationBuilder()
                .Build();

            Assert.Empty(configuration);
        }

        [Fact]
        public void Can_set_configuration_value()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfiguration("foo", "bar")
                .Build();

            AssertKeyValue(configuration, "foo", "bar");
        }

        private static void AssertKeyValue(IConfiguration configuration, string expectedKey, string expectedValue)
        {
            configuration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Can_build_empty_configuration_with_provider()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub()).Build();

            Assert.Empty(configuration);
        }

        [Fact]
        public void Can_pass_through_keys()
        {
            Assert.Equal("foo", ConfigurationBuilder.PassThroughKeyConverter("foo"));
        }

        [Fact]
        public void Can_ignore_values_from_configuration_provider()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string> {["foo"] = "bar"}))
                .Build();

            Assert.Empty(configuration);
        }

        [Fact]
        public void Can_use_configuration_value_from_provider()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string> {["group.id"] = "foo"}))
                .Build();

            AssertKeyValue(configuration, "group.id", "foo");
        }

        [Fact]
        public void Can_convert_key_with_environment_naming_convention()
        {
            Assert.Equal("GROUP_ID", ConfigurationBuilder.EnvVarStyleKeyConverter("group.id"));
        }

        [Fact]
        public void Can_use_configuration_value_from_provider_with_environment_naming_convention()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string> {["GROUP_ID"] = "foo"}), ConfigurationBuilder.EnvVarStyleKeyConverter)
                .Build();

            Assert.Equal("foo", configuration.First(x => x.Key == "group.id").Value);
        }

        [Fact]
        public void Can_convert_key_with_environment_naming_convention_and_prefix()
        {
            var sut = ConfigurationBuilder.EnvVarStyleKeyConverterWithPrefix("APP");

            var finalKey = sut("foo.bar");

            Assert.Equal("APP_FOO_BAR", finalKey);
        }

        [Fact]
        public void Can_use_configuration_value_from_provider_with_environment_naming_convention_and_prefix()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string> {["APP_GROUP_ID"] = "foo"}), ConfigurationBuilder.EnvVarStyleKeyConverterWithPrefix("APP"))
                .Build();

            Assert.Equal("foo", configuration.First(x => x.Key == "group.id").Value);
        }

        [Fact]
        public void Can_overwrite_values_from_provider()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfiguration("group.id", "bar")
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string> {["group.id"] = "foo"}))
                .Build();

            AssertKeyValue(configuration, "group.id", "bar");
        }

        [Fact]
        public void Can_get_values_from_multiple_providers()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string> {["bootstrap.servers"] = "foo"}))
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string> {["group.id"] = "bar"}))
                .Build();

            AssertKeyValue(configuration, "bootstrap.servers", "foo");
            AssertKeyValue(configuration, "group.id", "bar");
        }

        [Fact]
        public void Only_take_value_from_first_provider_that_matches()
        {
            var configuration = new ConfigurationBuilder()
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string> {["group.id"] = "foo"}))
                .WithConfigurationProvider(new ConfigurationProviderStub(new Dictionary<string, string> {["group.id"] = "bar"}))
                .Build();

            Assert.Equal("foo", configuration.First(x => x.Key == "group.id").Value);
        }
    }
}