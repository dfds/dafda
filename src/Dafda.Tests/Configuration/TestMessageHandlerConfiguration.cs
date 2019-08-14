using System.Security.Principal;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Messaging;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestMessageHandlerConfiguration
    {
        [Fact]
        public void Test1()
        {
            var spy = new MessageHandlerRegistry();
            new MessageHandlerConfiguration(new ServiceCollection(), spy)
                .FromTopic("foo")
                .OnMessage<DummyMessage, DummyMessageHandler>("bar");

            var messageRegistration = spy.GetRegistrationFor("bar");

            Assert.Equal("foo", messageRegistration.Topic);
            Assert.Equal("bar", messageRegistration.MessageType);
            Assert.Equal(typeof(DummyMessage), messageRegistration.MessageInstanceType);
            Assert.Equal(typeof(DummyMessageHandler), messageRegistration.HandlerInstanceType);
        }

        [Fact]
        public async Task Test2()
        {
            var services = new ServiceCollection().ConfigureConsumer(config =>
            {
                config.Handlers.FromTopic("foo").OnMessage<DummyMessage, DummyMessageHandler>("bar");

                config.WithConfiguration("group.id", "foo_group");
                config.WithConfiguration("bootstrap.servers", "localhost");
            });

            services.AddSingleton<DummyMessageHandler>();

            var provider = services.BuildServiceProvider();

            var handlerRegistry = provider.GetRequiredService<MessageHandlerRegistry>();

            var messageRegistration = handlerRegistry.GetRegistrationFor("bar");

            Assert.Equal("foo", messageRegistration.Topic);
            Assert.Equal("bar", messageRegistration.MessageType);
            Assert.Equal(typeof(DummyMessage), messageRegistration.MessageInstanceType);
            Assert.Equal(typeof(DummyMessageHandler), messageRegistration.HandlerInstanceType);

            var dispatcher = provider.GetRequiredService<ILocalMessageDispatcher>();

            var dummyMessage = new DummyMessage();
            await dispatcher.Dispatch(new TransportLevelMessageStub(dummyMessage, "bar"));

            var spy = provider.GetRequiredService<DummyMessageHandler>();

            Assert.Equal(dummyMessage, spy.LastHandledMessage);
        }

        public class DummyMessage
        {
        }

        public class DummyMessageHandler : IMessageHandler<DummyMessage>
        {
            public Task Handle(DummyMessage message)
            {
                LastHandledMessage = message;

                return Task.CompletedTask;
            }

            public object LastHandledMessage { get; private set; }
        }
    }

    public class DummyMessageHandlerSpy
    {
        public object LastHandledMessage { get; private set; }

        public void Handled(object message)
        {
            LastHandledMessage = message;
        }
    }
}