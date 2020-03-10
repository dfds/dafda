using Dafda.Configuration;
using Dafda.Producing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestProducerOptions
    {
        [Fact]
        public void Has_same_topic()
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var sut = new ProducerOptions(new ProducerConfigurationBuilder(), outgoingMessageRegistry);

            sut.For("topic")
                .Register<DummyMessage>("dummy", x => "id")
                .Register<AnotherDummyMessage>("another", x => "id")
                ;

            Assert.All(outgoingMessageRegistry, x => Assert.Equal("topic", x.Topic));
        }

        [Fact]
        public void Has_same_id()
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var sut = new OutboxOptions(new ServiceCollection(), outgoingMessageRegistry);

            sut.For("topic")
                .BaseOn<object>(x => "base-id")
                .Register<DummyMessage>("dummy")
                .Register<AnotherDummyMessage>("another")
                ;

            sut.Build();

            var keys = new[]
            {
                outgoingMessageRegistry.GetRegistration(new DummyMessage()).KeySelector(new DummyMessage()),
                outgoingMessageRegistry.GetRegistration(new AnotherDummyMessage()).KeySelector(new AnotherDummyMessage())
            };

            Assert.Equal(new[] {"base-id", "base-id"}, keys);
        }

        [Fact]
        public void Has_override_id()
        {
            var outgoingMessageRegistry = new OutgoingMessageRegistry();
            var sut = new OutboxOptions(new ServiceCollection(), outgoingMessageRegistry);

            sut.For("topic")
                .BaseOn<object>(x => "BaseId")
                .Register<DummyMessage>("dummy", o => "dummy-id")
                .Register<AnotherDummyMessage>("another", o => "another-id")
                ;

            sut.Build();

            var keys = new[]
            {
                outgoingMessageRegistry.GetRegistration(new DummyMessage()).KeySelector(new DummyMessage()),
                outgoingMessageRegistry.GetRegistration(new AnotherDummyMessage()).KeySelector(new AnotherDummyMessage())
            };

            Assert.Equal(new[] {"dummy-id", "another-id"}, keys);
        }

        public class DummyMessage
        {
        }

        public class AnotherDummyMessage
        {
        }
    }
}