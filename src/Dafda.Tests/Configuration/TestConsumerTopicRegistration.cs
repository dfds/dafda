using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConsumerTopicRegistration
    {
        [Fact]
        public void Has_same_topic()
        {
            var registry = new MessageHandlerRegistry();

            new ConsumerTopicRegistration(registry, "topic")
                .Register<DummyMessage, DummyMessageHandler>("dummy")
                .Register<AnotherDummyMessage, AnotherDummyMessageHandler>("another")
                ;

            Assert.All(registry.Registrations, x => Assert.Equal("topic", x.Topic));
        }

        public class DummyMessage
        {
        }

        public class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            public Task Handle(DummyMessage message, MessageHandlerContext context)
            {
                return Task.CompletedTask;
            }
        }

        public class AnotherDummyMessage
        {
        }

        public class AnotherDummyMessageHandler : IMessageHandler<AnotherDummyMessage>
        {
            public Task Handle(AnotherDummyMessage message, MessageHandlerContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}