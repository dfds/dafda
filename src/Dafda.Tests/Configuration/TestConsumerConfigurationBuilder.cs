using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
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
                .WithGroupId("foo")
                .WithBootstrapServers("bar")
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.GroupId, "foo");
            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        private static void AssertKeyValue(ConsumerConfiguration configuration, string expectedKey, string expectedValue)
        {
            configuration.KafkaConfiguration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Can_build_consumer_configuration()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithConfigurationSource(new ConfigurationSourceStub(
                    (key: "DEFAULT_KAFKA_GROUP_ID", value: "default_foo"),
                    (key: "SAMPLE_KAFKA_ENABLE_AUTO_COMMIT", value: "true"),
                    (key: ConfigurationKey.GroupId, value: "foo"),
                    (key: ConfigurationKey.BootstrapServers, value: "bar"),
                    (key: "dummy", value: "ignored")

                ))
                .WithNamingConvention(NamingConvention.Default)
                .WithEnvironmentStyle("DEFAULT_KAFKA", "SAMPLE_KAFKA")
                .WithConfiguration(ConfigurationKey.GroupId, "baz")
                .Build();

            AssertKeyValue(configuration, ConfigurationKey.GroupId, "baz");
            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
            AssertKeyValue(configuration, ConfigurationKey.EnableAutoCommit, "true");
            AssertKeyValue(configuration, "dummy", null);
        }

        [Fact]
        public void Can_register_message_handler()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithGroupId("foo")
                .WithBootstrapServers("bar")
                .RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage))
                .Build();

            var registration = configuration.MessageHandlerRegistry.GetRegistrationFor(nameof(DummyMessage));

            Assert.Equal(typeof(DummyMessageHandler), registration.HandlerInstanceType);
        }

        [Fact]
        public void returns_expected_auto_commit_when_not_set()
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithGroupId("foo")
                .WithBootstrapServers("bar")
                .Build();

            Assert.True(configuration.EnableAutoCommit);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("TRUE", true)]
        [InlineData("false", false)]
        [InlineData("FALSE", false)]
        public void returns_expected_auto_commit_when_configured_with_valid_value(string configValue, bool expected)
        {
            var configuration = new ConsumerConfigurationBuilder()
                .WithGroupId("foo")
                .WithBootstrapServers("bar")
                .WithConfiguration(ConfigurationKey.EnableAutoCommit, configValue)
                .Build();

            Assert.Equal(expected, configuration.EnableAutoCommit);
        }

        public class DummyMessage
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        private class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            public Task Handle(DummyMessage message, MessageHandlerContext context)
            {
                LastHandledMessage = message;

                return Task.CompletedTask;
            }

            public object LastHandledMessage { get; private set; }
        }
    }
}