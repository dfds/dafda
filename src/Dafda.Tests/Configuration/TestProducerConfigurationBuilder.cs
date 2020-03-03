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
                .WithBootstrapServers("foo")
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "foo");
        }

        private static void AssertKeyValue(ProducerConfiguration configuration, string expectedKey, string expectedValue)
        {
            configuration.KafkaConfiguration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Can_build_producer_configuration()
        {
            var configuration = new ProducerConfigurationBuilder()
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: ConfigurationKey.BootstrapServers, value: "foo"),
                    (key: ConfigurationKey.SaslUsername, value: "username"),
                    (key: ConfigurationKey.SaslMechanisms, value: "foo"),
                    (key: "DEFAULT_KAFKA_SASL_MECHANISMS", value: "default"),
                    (key: "SAMPLE_KAFKA_SASL_MECHANISMS", value: "sample"),
                    (key: "dummy", value: "ignored")
                ))
                .WithEnvironmentStyle("DEFAULT_KAFKA", "SAMPLE_KAFKA")
                .WithNamingConvention(NamingConvention.Default)
                .WithConfiguration(ConfigurationKey.BootstrapServers, "bar")
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
            AssertKeyValue(configuration, ConfigurationKey.SaslUsername, "username");
            AssertKeyValue(configuration, ConfigurationKey.SaslMechanisms, "default");
            AssertKeyValue(configuration, "dummy", null);
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