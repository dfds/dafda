using System.Collections.Generic;
using System.Linq;
using Dafda.Configuration;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConfigurationBuilder
    {
        #region Test Helpers

        private static readonly string[] None = new string[0];

        private static string[] ConfigurationKeys(params string[] keys)
        {
            return keys;
        }

        private static void AssertKeyValue(IDictionary<string, string> configuration, string expectedKey, string expectedValue)
        {
            configuration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        #endregion

        [Fact]
        public void Can_validate_configuration_when_require_key_is_missing()
        {
            var sut = new ConfigurationBuilder().WithRequiredConfigurationKeys("some-key");

            Assert.Throws<InvalidConfigurationException>(() => sut.Build());
        }

        [Fact]
        public void Can_create_empty_configuration()
        {
            var configuration = new ConfigurationBuilder(ConfigurationKeys("key1"), None)
                .Build();

            Assert.Empty(configuration);
        }

        [Fact]
        public void Can_use_configurations()
        {
            var configuration = new ConfigurationBuilder(None, None)
                .WithConfigurations(new Dictionary<string, string>
                {
                    {"key1", "foo"},
                })
                .Build();

            AssertKeyValue(configuration, "key1", "foo");
        }

        [Fact]
        public void Can_ignore_out_of_scope_values_from_configuration_source()
        {
            var configuration = new ConfigurationBuilder(None, None)
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: "dummy", value: "baz")
                )).Build();

            AssertKeyValue(configuration, "dummy", null);
        }

        [Fact]
        public void Can_use_configuration_value_from_source()
        {
            var configuration = new ConfigurationBuilder(ConfigurationKeys("key1"), None)
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: "key1", value: "foo")
                ))
                .Build();

            AssertKeyValue(configuration, "key1", "foo");
        }

        [Fact]
        public void Can_use_configuration_value_from_source_with_environment_naming_convention()
        {
            var configuration = new ConfigurationBuilder(ConfigurationKeys("key1"), None)
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: "KEY1", value: "foo")
                ))
                .WithNamingConventions(NamingConvention.UseCustom(x => x.ToUpper()))
                .Build();

            AssertKeyValue(configuration, "key1", "foo");
        }

        [Fact]
        public void Manually_added_values_takes_precedence()
        {
            var configuration = new ConfigurationBuilder(ConfigurationKeys("key1"), None)
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: "key1", value: "foo")
                ))
                .WithConfigurations(new Dictionary<string, string>
                {
                    {"key1", "baz"},
                })
                .Build();

            AssertKeyValue(configuration, "key1", "baz");
        }

        [Fact]
        public void Use_all_naming_conventions_when_searching_for_key()
        {
            var configuration = new ConfigurationBuilder(ConfigurationKeys("key1"), None)
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: "KEY1", value: "foo")
                ))
                .WithNamingConventions(NamingConvention.Default, NamingConvention.UseCustom(x => x.ToUpper()))
                .Build();

            AssertKeyValue(configuration, "key1", "foo");
        }

        [Fact]
        public void Only_take_value_from_first_source_that_matches()
        {
            var configuration = new ConfigurationBuilder(ConfigurationKeys("key1"), None)
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: "KEY1", value: "foo"),
                    (key: "key1", value: "bar")
                ))
                .WithNamingConventions(NamingConvention.UseCustom(x => x.ToUpper()), NamingConvention.Default)
                .Build();

            AssertKeyValue(configuration, "key1", "foo");
        }
    }
}