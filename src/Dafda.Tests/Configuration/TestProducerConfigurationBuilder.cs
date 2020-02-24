using System.Linq;
using Dafda.Configuration;
using Dafda.Producing;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestProducerConfigurationBuilder
    {
        [Fact]
        public void Can_validate_configuration()
        {
            var sut = new ProducerConfigurationBuilder();

            Assert.Throws<InvalidConfigurationException>(() => sut.Build());
        }

        [Fact]
        public void Can_build_minimal_configuration()
        {
            var configuration = new ProducerConfigurationBuilder()
                .WithBootstrapServers("bar")
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        private static void AssertKeyValue(ProducerConfiguration configuration, string expectedKey, string expectedValue)
        {
            configuration.KafkaConfiguration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Can_ignore_out_of_scope_values_from_configuration_source()
        {
            var configuration = new ProducerConfigurationBuilder()
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: ConfigurationKey.BootstrapServers, value: "bar"),
                    (key: "dummy", value: "baz")
                ))
                .Build();

            AssertKeyValue(configuration, "dummy", null);
        }

        [Fact]
        public void Can_use_configuration_value_from_source()
        {
            var configuration = new ProducerConfigurationBuilder()
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: ConfigurationKey.BootstrapServers, value: "bar")
                ))
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_source_with_environment_naming_convention()
        {
            var configuration = new ProducerConfigurationBuilder()
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: "BOOTSTRAP_SERVERS", value: "bar")
                ))
                .WithEnvironmentStyle()
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_source_with_environment_naming_convention_and_prefix()
        {
            var configuration = new ProducerConfigurationBuilder()
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: "DEFAULT_KAFKA_BOOTSTRAP_SERVERS", value: "bar")
                ))
                .WithEnvironmentStyle("DEFAULT_KAFKA")
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_overwrite_values_from_source()
        {
            var configuration = new ProducerConfigurationBuilder()
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: ConfigurationKey.BootstrapServers, value: "foo")
                ))
                .WithConfiguration(ConfigurationKey.BootstrapServers, "bar")
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Only_take_value_from_first_source_that_matches()
        {
            var configuration = new ProducerConfigurationBuilder()
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: ConfigurationKey.BootstrapServers, value: "foo"),
                    (key: "BOOTSTRAP_SERVERS", value: "bar")
                ))
                .WithNamingConvention(NamingConvention.Default)
                .WithEnvironmentStyle()
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "foo");
        }

        [Fact]
        public void Has_expected_message_id_generator()
        {
            var dummy = MessageIdGenerator.Default;

            var producerConfiguration = new ProducerConfigurationBuilder()
                .WithBootstrapServers("foo")
                .WithMessageIdGenerator(dummy)
                .Build();

            Assert.Equal(dummy, producerConfiguration.MessageIdGenerator);
        }
    }
}