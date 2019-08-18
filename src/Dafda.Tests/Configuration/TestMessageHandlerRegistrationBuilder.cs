using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestMessageHandlerRegistrationBuilder
    {
        [Fact]
        public void Can_register_message_handler()
        {
            var spy = new MessageHandlerRegistry();
            var sut = new MessageHandlerRegistrationBuilder(new ServiceCollection(), spy);
            sut.AddMessageHandlers(topicBuilder => topicBuilder.FromTopic("foo").OnMessage<DummyMessage, DummyMessageHandler>("bar"));

            var messageRegistration = spy.GetRegistrationFor("bar");

            Assert.Equal("foo", messageRegistration.Topic);
            Assert.Equal("bar", messageRegistration.MessageType);
            Assert.Equal(typeof(DummyMessage), messageRegistration.MessageInstanceType);
            Assert.Equal(typeof(DummyMessageHandler), messageRegistration.HandlerInstanceType);
        }

        private class DummyMessage
        {
        }

        private class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            public Task Handle(DummyMessage message)
            {
                return Task.CompletedTask;
            }
        }
    }
}