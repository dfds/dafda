using Dafda.Configuration;
using Dafda.Producing;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestProducerTopicRegistration
    {
        [Fact]
        public void Has_same_topic()
        {
            var registry = new OutgoingMessageRegistry();

            new ProducerTopicRegistration(registry, "topic")
                .Register<DummyMessage>("dummy", x => "id")
                .Register<AnotherDummyMessage>("another", x => "id")
                ;

            Assert.All(registry, x => Assert.Equal("topic", x.Topic));
        }

        [Fact]
        public void Has_same_id()
        {
            var registry = new OutgoingMessageRegistry();

            new ProducerTopicRegistration(registry, "topic")
                .BaseOn<object>(x => "base-id")
                .Register<DummyMessage>("dummy")
                .Register<AnotherDummyMessage>("another")
                ;

            var keys = new[]
            {
                registry.GetRegistration(new DummyMessage()).KeySelector(new DummyMessage()),
                registry.GetRegistration(new AnotherDummyMessage()).KeySelector(new AnotherDummyMessage())
            };

            Assert.Equal(new[] {"base-id", "base-id"}, keys);
        }

        [Fact]
        public void Has_override_id()
        {
            var registry = new OutgoingMessageRegistry();

            new ProducerTopicRegistration(registry, "topic")
                .BaseOn<object>(x => "BaseId")
                .Register<DummyMessage>("dummy", o => "dummy-id")
                .Register<AnotherDummyMessage>("another", o => "another-id")
                ;

            var keys = new[]
            {
                registry.GetRegistration(new DummyMessage()).KeySelector(new DummyMessage()),
                registry.GetRegistration(new AnotherDummyMessage()).KeySelector(new AnotherDummyMessage())
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