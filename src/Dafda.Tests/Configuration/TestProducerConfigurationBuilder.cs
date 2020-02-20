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
            var sut = new ProducerConfigurationBuilder();
            sut.WithBootstrapServers("bar");

            var configuration = sut.Build();

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
            var sut = new ProducerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: ConfigurationKey.BootstrapServers, value: "bar"),
                (key: "dummy", value: "baz")
            ));

            var configuration = sut.Build();

            AssertKeyValue(configuration, "dummy", null);
        }

        [Fact]
        public void Can_use_configuration_value_from_source()
        {
            var sut = new ProducerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: ConfigurationKey.BootstrapServers, value: "bar")
            ));

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_source_with_environment_naming_convention()
        {
            var sut = new ProducerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: "BOOTSTRAP_SERVERS", value: "bar")
            ));
            sut.WithEnvironmentStyle();

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_source_with_environment_naming_convention_and_prefix()
        {
            var sut = new ProducerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: "DEFAULT_KAFKA_BOOTSTRAP_SERVERS", value: "bar")
            ));
            sut.WithEnvironmentStyle("DEFAULT_KAFKA");

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_overwrite_values_from_source()
        {
            var sut = new ProducerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: ConfigurationKey.BootstrapServers, value: "foo")
            ));
            sut.WithConfiguration(ConfigurationKey.BootstrapServers, "bar");

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Only_take_value_from_first_source_that_matches()
        {
            var sut = new ProducerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: ConfigurationKey.BootstrapServers, value: "foo"),
                (key: "BOOTSTRAP_SERVERS", value: "bar")
            ));
            sut.WithNamingConvention(NamingConvention.Default);
            sut.WithEnvironmentStyle();

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "foo");
        }

        [Fact]
        public void Has_expected_message_id_generator()
        {
            var dummy = MessageIdGenerator.Default;

            var sut = new ProducerConfigurationBuilder();
            sut.WithBootstrapServers("foo");
            sut.WithMessageIdGenerator(dummy);
            var producerConfiguration = sut.Build();

            Assert.Equal(dummy, producerConfiguration.MessageIdGenerator);
        }
    }
}