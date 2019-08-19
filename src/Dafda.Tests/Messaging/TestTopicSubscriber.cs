using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Messaging;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Moq;
using Xunit;
using TopicSubscriber = Dafda.Messaging.TopicSubscriber;

namespace Dafda.Tests.Messaging
{
    public class TestTopicSubscriber
    {
        [Fact]
        public async Task expected_handler_is_invoked()
        {
            var mock = new Mock<IMessageHandler<FooMessage>>();

            var handlerRegistry = new MessageHandlerRegistry();
            handlerRegistry.Register<FooMessage, FooHandler>("dummy topic", "foo");

            var typeResolver = new TypeResolverStub(mock.Object);

            var localMessageDispatcher = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(handlerRegistry)
                .WithHandlerUnitOfWorkFactory(new DefaultUnitOfWorkFactory(typeResolver))
                .Build();

            var consumerFactory = Dummy.Of<IConsumerFactory>();
            var sut = new TopicSubscriber(consumerFactory, localMessageDispatcher);

            IConsumer consumer = new TestConsumer();

            await sut.ProcessNextMessage(consumer, CancellationToken.None);

            mock.Verify(x => x.Handle(It.IsAny<FooMessage>()), Times.Once);
        }

        #region private helper classes

        public class FooMessage
        {
            public string Value { get; set; }
        }

        public class FooHandler : IMessageHandler<FooMessage>
        {
            public Task Handle(FooMessage message)
            {
                return Task.CompletedTask;
            }
        }

        #endregion
    }

    public class TestConsumer : IConsumer
    {
        public ConsumeResult Consume(CancellationToken cancellationToken)
        {
            return new ConsumeResult("{\"type\": \"foo\", \"data\": {\"value\":\"bar\"}}");
        }

        public void Dispose()
        {
            
        }
    }
}