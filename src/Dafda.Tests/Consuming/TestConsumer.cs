using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Messaging;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Moq;
using Xunit;

namespace Dafda.Tests.Consuming
{
    public class TestConsumer
    {
        [Fact]
        public async Task invokes_expected_handler_when_consuming()
        {
            var handlerMock = new Mock<IMessageHandler<FooMessage>>();
            var handlerStub = handlerMock.Object;

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var sut = new ConsumerBuilder()
                .WithUnitOfWorkFactory(type => new UnitOfWorkStub(handlerStub))
                .WithMessageRegistrations(messageRegistrationStub)
                .Build();

            await sut.ConsumeSingle(CancellationToken.None);

            handlerMock.Verify(x => x.Handle(It.IsAny<FooMessage>()), Times.Once);
        }

        [Fact]
        public async Task throws_expected_exception_when_consuming_a_message_without_a_handler_as_been_registered_for_it()
        {
            var sut = new ConsumerBuilder().Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(() => sut.ConsumeSingle(CancellationToken.None));
        }

        [Fact]
        public async Task expected_order_of_handler_invocation_in_unit_of_work()
        {
            var orderOfInvocation = new LinkedList<string>();

            var dummyMessageResult = new MessageResultBuilder().Build();
            var dummyMessageRegistration = new MessageRegistrationBuilder().Build();

            var sut = new ConsumerBuilder()
                .WithUnitOfWorkFactory(type => new UnitOfWorkSpy(
                    handlerInstance: new MessageHandlerSpy<FooMessage>(() => orderOfInvocation.AddLast("during")),
                    pre: () => orderOfInvocation.AddLast("before"),
                    post: () => orderOfInvocation.AddLast("after")
                ))
                .WithTopicSubscriberScopeFactory(new TopicSubscriberScopeFactoryStub(new TopicSubscriberScopeStub(dummyMessageResult)))
                .WithMessageRegistrations(dummyMessageRegistration)
                .Build();

            await sut.ConsumeSingle(CancellationToken.None);

            Assert.Equal(new[] {"before", "during", "after"}, orderOfInvocation);
        }

        [Fact]
        public async Task will_not_call_commit_when_auto_commit_is_enabled()
        {
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var wasCalled = false;

            var resultSpy = new MessageResultBuilder()
                .WithOnCommit(() =>
                {
                    wasCalled = true;
                    return Task.CompletedTask;
                })
                .Build();

            var topicSubscriberScopeFactoryStub = new TopicSubscriberScopeFactoryStub(new TopicSubscriberScopeStub(resultSpy));
            var consumer = new ConsumerBuilder()
                .WithTopicSubscriberScopeFactory(topicSubscriberScopeFactoryStub)
                .WithUnitOfWorkFactory(x => new UnitOfWorkStub(handlerStub))
                .WithMessageRegistrations(messageRegistrationStub)
                .WithEnableAutoCommit(true)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task will_call_commit_when_auto_commit_is_disabled()
        {
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var wasCalled = false;

            var resultSpy = new MessageResultBuilder()
                .WithOnCommit(() =>
                {
                    wasCalled = true;
                    return Task.CompletedTask;
                })
                .Build();

            var topicSubscriberScopeFactoryStub = new TopicSubscriberScopeFactoryStub(new TopicSubscriberScopeStub(resultSpy));
            var consumer = new ConsumerBuilder()
                .WithTopicSubscriberScopeFactory(topicSubscriberScopeFactoryStub)
                .WithUnitOfWorkFactory(x => new UnitOfWorkStub(handlerStub))
                .WithMessageRegistrations(messageRegistrationStub)
                .WithEnableAutoCommit(false)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.True(wasCalled);
        }

        #region helper classes

        public class FooMessage
        {
            public string Value { get; set; }
        }

        #endregion
    }
}