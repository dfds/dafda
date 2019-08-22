using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Messaging;
using Dafda.Tests.Builders;
using Dafda.Tests.Messaging;
using Dafda.Tests.TestDoubles;
using Moq;
using Xunit;

namespace Dafda.Tests
{
    public class TestEndToEnd
    {
        [Fact]
        public async Task duno()
        {
            var mock = new Mock<IMessageHandler<FooMessage>>();

            var configuration = new ConsumerConfigurationBuilder()
                .WithGroupId("foo")
                .WithBootstrapServers("bar")
                .WithUnitOfWorkFactory(new DefaultUnitOfWorkFactory(new TypeResolverStub(mock.Object)))
                .RegisterMessageHandler<FooMessage, IMessageHandler<FooMessage>>("dummy-topic", "foo")
                .Build();

            var localMessageDispatcher = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(configuration.MessageHandlerRegistry)
                .WithHandlerUnitOfWorkFactory(configuration.UnitOfWorkFactory)
                .Build();

            var sut = new TopicSubscriber(
                consumerFactory: Dummy.Of<IConsumerFactory>(),
                localMessageDispatcher: localMessageDispatcher
            );

            await sut.ProcessNextMessage(
                consumer: new TestConsumer("foo"),
                cancellationToken: CancellationToken.None
            );

            mock.Verify(x => x.Handle(It.IsAny<FooMessage>()), Times.Once);
        }

        #region private helper classes

        public class FooMessage
        {
            public string Value { get; set; }
        }

        #endregion
    }
}