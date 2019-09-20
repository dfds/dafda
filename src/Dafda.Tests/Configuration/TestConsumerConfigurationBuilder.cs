using System.Linq;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Messaging;
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
            var sut = new ConsumerConfigurationBuilder();
            sut.WithGroupId("foo");
            sut.WithBootstrapServers("bar");

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.GroupId, "foo");
            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        private static void AssertKeyValue(IConfiguration configuration, string expectedKey, string expectedValue)
        {
            configuration.FirstOrDefault(x => x.Key == expectedKey).Deconstruct(out _, out var actualValue);

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void Can_ignore_out_of_scope_values_from_configuration_source()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: ConfigurationKey.GroupId, value: "foo"),
                (key: ConfigurationKey.BootstrapServers, value: "bar"),
                (key: "dummy", value: "baz")
            ));

            var configuration = sut.Build();

            AssertKeyValue(configuration, "dummy", null);
        }

        [Fact]
        public void Can_use_configuration_value_from_source()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: ConfigurationKey.GroupId, value: "foo"),
                (key: ConfigurationKey.BootstrapServers, value: "bar")
            ));

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.GroupId, "foo");
            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_source_with_environment_naming_convention()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: "GROUP_ID", value: "foo"),
                (key: "BOOTSTRAP_SERVERS", value: "bar")
            ));
            sut.WithEnvironmentStyle();

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.GroupId, "foo");
            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_use_configuration_value_from_source_with_environment_naming_convention_and_prefix()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: "DEFAULT_KAFKA_GROUP_ID", value: "foo"),
                (key: "DEFAULT_KAFKA_BOOTSTRAP_SERVERS", value: "bar")
            ));
            sut.WithEnvironmentStyle("DEFAULT_KAFKA");

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.GroupId, "foo");
            AssertKeyValue(configuration, ConfigurationKey.BootstrapServers, "bar");
        }

        [Fact]
        public void Can_overwrite_values_from_source()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: ConfigurationKey.GroupId, value: "foo"),
                (key: ConfigurationKey.BootstrapServers, value: "bar")
            ));
            sut.WithConfiguration(ConfigurationKey.GroupId, "baz");

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.GroupId, "baz");
        }

        [Fact]
        public void Only_take_value_from_first_source_that_matches()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithConfigurationSource(new ConfigurationSourceStub(
                (key: ConfigurationKey.GroupId, value: "foo"),
                (key: ConfigurationKey.BootstrapServers, value: "bar"),
                (key: "GROUP_ID", value: "baz")
            ));
            sut.WithNamingConvention(NamingConvention.Default);
            sut.WithEnvironmentStyle();

            var configuration = sut.Build();

            AssertKeyValue(configuration, ConfigurationKey.GroupId, "foo");
        }

        [Fact]
        public void Can_register_message_handler()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithGroupId("foo");
            sut.WithBootstrapServers("bar");
            sut.RegisterMessageHandler<DummyMessage, DummyMessageHandler>("dummyTopic", nameof(DummyMessage));

            var configuration = sut.Build();

            var registration = configuration.MessageHandlerRegistry.GetRegistrationFor(nameof(DummyMessage));

            Assert.Equal(typeof(DummyMessageHandler), registration.HandlerInstanceType);
        }

        [Fact]
        public void Has_default_commit_strategy()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithGroupId("foo");
            sut.WithBootstrapServers("bar");
            var configuration = sut.Build();
            Assert.IsType<AlwaysCommit>(configuration.CommitStrategy);
        }

        [Fact]
        public void Can_set_AutoCommitOnly_stategy()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithGroupId("foo");
            sut.WithBootstrapServers("bar");
            sut.WithAutoCommitOnly(999);
            var configuration = sut.Build();
            Assert.IsType<NeverCommit>(configuration.CommitStrategy);
            AssertKeyValue(configuration, ConfigurationKey.EnableAutoCommit, "true");
            AssertKeyValue(configuration, ConfigurationKey.AutoCommitInterval, "999");
        }

        [Fact]
        public void Can_set_NeverCommit_stategy()
        {
            var sut = new ConsumerConfigurationBuilder();
            sut.WithGroupId("foo");
            sut.WithBootstrapServers("bar");
            sut.WithManualCommitOnly();
            var configuration = sut.Build();
            Assert.IsType<AlwaysCommit>(configuration.CommitStrategy);
            AssertKeyValue(configuration, ConfigurationKey.EnableAutoCommit, "false");
        }

        public class DummyMessage
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        private class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            public Task Handle(DummyMessage message)
            {
                LastHandledMessage = message;

                return Task.CompletedTask;
            }

            public object LastHandledMessage { get; private set; }
        }
    }
}