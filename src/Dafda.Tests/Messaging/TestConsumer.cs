using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Messaging;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Moq;
using Xunit;

namespace Dafda.Tests.Messaging
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
                .WithUnitOfWorkFactory(type => new DirectUnitOfWork(handlerStub))
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

            Assert.Equal(new[] { "before", "during", "after" }, orderOfInvocation);
        }

        #region helper classes

        public class FooMessage
        {
            public string Value { get; set; }
        }

        #endregion
    }
}