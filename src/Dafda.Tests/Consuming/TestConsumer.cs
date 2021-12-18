using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Microsoft.Extensions.Logging;
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

            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var sut = new ConsumerBuilder()
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
                .Build();

            await sut.ConsumeSingle(CancellationToken.None);

            handlerMock.Verify(x => x.Handle(It.IsAny<FooMessage>(), It.IsAny<MessageHandlerContext>()), Times.Once);
        }

        [Fact]
        public async Task throws_when_consuming_an_unknown_message_when_explicit_handlers_are_required()
        {
            var sut = new ConsumerBuilder().Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(
                () => sut.ConsumeSingle(CancellationToken.None));
        }

        [Fact]
        public async Task does_not_throw_when_consuming_an_unknown_message_with_no_op_strategy()
        {
            var sut =
                new ConsumerBuilder()
                    .WithUnitOfWork(
                        new UnitOfWorkStub(
                            new NoOpHandler(new Mock<ILogger<NoOpHandler>>().Object)))
                    .WithUnconfiguredMessageStrategy(new UseNoOpHandler())
                    .Build();

            await sut.ConsumeSingle(CancellationToken.None);
        }

        [Fact]
        public async Task expected_order_of_handler_invocation_in_unit_of_work()
        {
            var orderOfInvocation = new LinkedList<string>();

            var dummyMessageResult = new MessageResultBuilder().WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build()).Build();
            var dummyMessageRegistration = new MessageRegistrationBuilder().WithMessageType("foo").Build();

            var registry = new MessageHandlerRegistry();
            registry.Register(dummyMessageRegistration);

            var sut = new ConsumerBuilder()
                .WithUnitOfWork(new UnitOfWorkSpy(
                    handlerInstance: new MessageHandlerSpy<FooMessage>(() => orderOfInvocation.AddLast("during")),
                    pre: () => orderOfInvocation.AddLast("before"),
                    post: () => orderOfInvocation.AddLast("after")
                ))
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(dummyMessageResult)))
                .WithMessageHandlerRegistry(registry)
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
            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(consumerScopeFactoryStub)
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
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
            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(consumerScopeFactoryStub)
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
                .WithEnableAutoCommit(false)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);

            Assert.True(wasCalled);
        }

        [Fact]
        public async Task creates_consumer_scope_when_consuming_single_message()
        {
            var messageResultStub = new MessageResultBuilder().WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build()).Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var spy = new ConsumerScopeFactorySpy(new ConsumerScopeStub(messageResultStub));

            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(spy)
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);


            Assert.Equal(1, spy.CreateConsumerScopeCalled);
        }

        [Fact]
        public async Task disposes_consumer_scope_when_consuming_single_message()
        {
            var messageResultStub = new MessageResultBuilder().WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build()).Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            var spy = new ConsumerScopeSpy(messageResultStub);

            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var consumer = new ConsumerBuilder()
                .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(spy))
                .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                .WithMessageHandlerRegistry(registry)
                .Build();

            await consumer.ConsumeSingle(CancellationToken.None);


            Assert.Equal(1, spy.Disposed);
        }

        [Fact]
        public async Task creates_consumer_scope_when_consuming_multiple_messages()
        {
            var messageResultStub = new MessageResultBuilder().WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build()).Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
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

                var spy = new ConsumerScopeFactorySpy(subscriberScopeStub);

                var registry = new MessageHandlerRegistry();
                registry.Register(messageRegistrationStub);

                var consumer = new ConsumerBuilder()
                    .WithConsumerScopeFactory(spy)
                    .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                    .WithMessageHandlerRegistry(registry)
                    .Build();

                await consumer.ConsumeAll(cancellationTokenSource.Token);

                Assert.Equal(2, loops);
                Assert.Equal(1, spy.CreateConsumerScopeCalled);
            }
        }

        [Fact]
        public async Task disposes_consumer_scope_when_consuming_multiple_messages()
        {
            var messageResultStub = new MessageResultBuilder().WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build()).Build();
            var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

            var messageRegistrationStub = new MessageRegistrationBuilder()
                .WithHandlerInstanceType(handlerStub.GetType())
                .WithMessageInstanceType(typeof(FooMessage))
                .WithMessageType("foo")
                .Build();

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                var loops = 0;

                var spy = new ConsumerScopeSpy(messageResultStub, () =>
                {
                    loops++;

                    if (loops == 2)
                    {
                        cancellationTokenSource.Cancel();
                    }
                });

                var registry = new MessageHandlerRegistry();
                registry.Register(messageRegistrationStub);

                var consumer = new ConsumerBuilder()
                    .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(spy))
                    .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
                    .WithMessageHandlerRegistry(registry)
                    .Build();

                await consumer.ConsumeAll(cancellationTokenSource.Token);

                Assert.Equal(2, loops);
                Assert.Equal(1, spy.Disposed);
            }
        }

        #region helper classes

        private class ConsumerScopeDecoratorWithHooks : ConsumerScope
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

    internal class ConsumerScopeSpy : ConsumerScope
    {
        private readonly MessageResult _messageResult;
        private readonly Action _onGetNext;

        public ConsumerScopeSpy(MessageResult messageResult, Action onGetNext = null)
        {
            _messageResult = messageResult;
            _onGetNext = onGetNext;
        }

        public override Task<MessageResult> GetNext(CancellationToken cancellationToken)
        {
            _onGetNext?.Invoke();

            return Task.FromResult(_messageResult);
        }

        public override void Dispose()
        {
            Disposed++;
        }

        public int Disposed { get; private set; }
    }
}
