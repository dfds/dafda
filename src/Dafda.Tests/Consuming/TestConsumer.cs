using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
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
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(dummyMessageResult)))
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

            var consumerScopeFactoryStub = new ConsumerScopeFactoryStub(new ConsumerScopeStub(resultSpy));
            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(consumerScopeFactoryStub)
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

            var consumerScopeFactoryStub = new ConsumerScopeFactoryStub(new ConsumerScopeStub(resultSpy));
            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(consumerScopeFactoryStub)
                .WithUnitOfWorkFactory(x => new UnitOfWorkStub(handlerStub))
                .WithMessageRegistrations(messageRegistrationStub)
                .WithEnableAutoCommit(false)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.True(wasCalled);
        }

        [Fact]
        public async Task creates_consumer_scope_when_consuming_single_message()
        {
            var messageResultStub = new MessageResultBuilder().Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType(messageResultStub.Message.Type)
                .Build();

            var mock = new Mock<IConsumerScopeFactory>();
            mock
                .Setup(x => x.CreateConsumerScope())
                .Returns(new ConsumerScopeStub(messageResultStub));

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(mock.Object)
                .WithUnitOfWorkFactory(x => new UnitOfWorkStub(handlerStub))
                .WithMessageRegistrations(messageRegistrationStub)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);

            mock.Verify(x => x.CreateConsumerScope(), Times.Once);
        }

        [Fact]
        public async Task disposes_consumer_scope_when_consuming_single_message()
        {
            var messageResultStub = new MessageResultBuilder().Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType(messageResultStub.Message.Type)
                .Build();

            var mock = new Mock<ConsumerScope>();
            mock
                .Setup(x => x.GetNext(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(messageResultStub));

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(mock.Object))
                .WithUnitOfWorkFactory(x => new UnitOfWorkStub(handlerStub))
                .WithMessageRegistrations(messageRegistrationStub)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);

            mock.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task creates_consumer_scope_when_consuming_multiple_messages()
        {
            var messageResultStub = new MessageResultBuilder().Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType(messageResultStub.Message.Type)
                .Build();

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                var loops = 0;

                var subscriberScopeStub = new ConsumerScopeDecoratorWithHooks(
                    inner: new ConsumerScopeStub(messageResultStub),
                    postHook: () =>
                    {
                        loops++;

                        if (loops == 2)
                        {
                            cancellationTokenSource.Cancel();
                        }
                    }
                );

                var mock = new Mock<IConsumerScopeFactory>();

                mock
                    .Setup(x => x.CreateConsumerScope())
                    .Returns(subscriberScopeStub);

                var consumer = new ConsumerBuilder()
                    .WithConsumerScopeFactory(mock.Object)
                    .WithUnitOfWorkFactory(x => new UnitOfWorkStub(handlerStub))
                    .WithMessageRegistrations(messageRegistrationStub)
                    .Build();

                await consumer.ConsumeAll(cancellationTokenSource.Token);

                Assert.Equal(2, loops);
                mock.Verify(x => x.CreateConsumerScope(), Times.Once);
            }
        }

        [Fact]
        public async Task disposes_consumer_scope_when_consuming_multiple_messages()
        {
            var messageResultStub = new MessageResultBuilder().Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType(messageResultStub.Message.Type)
                .Build();

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                var loops = 0;

                var mock = new Mock<ConsumerScope>();
                mock
                    .Setup(x => x.GetNext(It.IsAny<CancellationToken>()))
                    .Callback(() =>
                    {
                        loops++;

                        if (loops == 2)
                        {
                            cancellationTokenSource.Cancel();
                        }
                    })
                    .Returns(Task.FromResult(messageResultStub));

                var consumer = new ConsumerBuilder()
                    .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(mock.Object))
                    .WithUnitOfWorkFactory(x => new UnitOfWorkStub(handlerStub))
                    .WithMessageRegistrations(messageRegistrationStub)
                    .Build();

                await consumer.ConsumeAll(cancellationTokenSource.Token);

                Assert.Equal(2, loops);

                mock.Verify(x => x.Dispose(), Times.Once);
            }
        }

        #region helper classes

        public class ConsumerScopeDecoratorWithHooks : ConsumerScope
        {
            private readonly ConsumerScope _inner;
            private readonly Action _preHook;
            private readonly Action _postHook;

            public ConsumerScopeDecoratorWithHooks(ConsumerScope inner, Action preHook = null, Action postHook = null)
            {
                _inner = inner;
                _preHook = preHook;
                _postHook = postHook;
            }

            public override async Task<MessageResult> GetNext(CancellationToken cancellationToken)
            {
                _preHook?.Invoke();
                var result = await _inner.GetNext(cancellationToken);
                _postHook?.Invoke();

                return result;
            }

            public override void Dispose()
            {
                _inner.Dispose();
            }
        }

        public class FooMessage
        {
            public string Value { get; set; }
        }

        #endregion
    }
}