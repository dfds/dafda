namespace Dafda.Tests.Consuming;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Builders;
using Dafda.Consuming;
using Microsoft.Extensions.Logging;
using Moq;
using TestDoubles;
using Xunit;

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
            .WithTopic("")
            .Build();

        var registry = new MessageHandlerRegistry();
        registry.Register(messageRegistrationStub);

        var sut = new ConsumerBuilder()
            .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
            .WithMessageHandlerRegistry(registry)
            .Build();

        await sut.ConsumeSingle(CancellationToken.None);

        handlerMock.Verify(x => x.Handle(It.IsAny<FooMessage>(), It.IsAny<MessageHandlerContext>(), It.IsAny<CancellationToken>()), Times.Once);
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

        var dummyMessageResult = new MessageResultBuilder()
            .WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build())
            .WithTopic("topic")
            .Build();
        var dummyMessageRegistration = new MessageRegistrationBuilder()
            .WithMessageType("foo")
            .WithTopic("topic")
            .Build();

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

        Assert.Equal(new[] { "before", "during", "after" }, orderOfInvocation);
    }

    [Fact]
    public async Task will_not_call_commit_when_auto_commit_is_enabled()
    {
        var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

        var messageRegistrationStub = new MessageRegistrationBuilder()
            .WithHandlerInstanceType(handlerStub.GetType())
            .WithMessageInstanceType(typeof(FooMessage))
            .WithMessageType("foo")
            .WithTopic("topic")
            .Build();

        var wasCalled = false;

        var resultSpy = new MessageResultBuilder()
            .WithOnCommit((_) =>
            {
                wasCalled = true;
                return Task.CompletedTask;
            })
            .WithTopic("topic")
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
            .WithTopic("topic")
            .Build();

        var wasCalled = false;

        var resultSpy = new MessageResultBuilder()
            .WithTopic("topic")
            .WithOnCommit((_) =>
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
        var messageResultStub = new MessageResultBuilder()
            .WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build())
            .WithTopic("topic")
            .Build();
        var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

        var messageRegistrationStub = new MessageRegistrationBuilder()
            .WithHandlerInstanceType(handlerStub.GetType())
            .WithMessageInstanceType(typeof(FooMessage))
            .WithMessageType("foo")
            .WithTopic("topic")
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
        var messageResultStub = new MessageResultBuilder()
            .WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build())
            .WithTopic("topic")
            .Build();
        var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

        var messageRegistrationStub = new MessageRegistrationBuilder()
            .WithHandlerInstanceType(handlerStub.GetType())
            .WithMessageInstanceType(typeof(FooMessage))
            .WithMessageType("foo")
            .WithTopic("topic")
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
        var messageResultStub = new MessageResultBuilder()
            .WithTransportLevelMessage(new TransportLevelMessageBuilder()
                .WithType("foo")
                .Build())
            .WithTopic("topic")
            .Build();
        var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

        var messageRegistrationStub = new MessageRegistrationBuilder()
            .WithHandlerInstanceType(handlerStub.GetType())
            .WithMessageInstanceType(typeof(FooMessage))
            .WithMessageType("foo")
            .WithTopic("topic")
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
        var messageResultStub = new MessageResultBuilder()
            .WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build())
            .WithTopic("topic")
            .Build();
        var handlerStub = Dummy.Of<IMessageHandler<FooMessage>>();

        var messageRegistrationStub = new MessageRegistrationBuilder()
            .WithHandlerInstanceType(handlerStub.GetType())
            .WithMessageInstanceType(typeof(FooMessage))
            .WithMessageType("foo")
            .WithTopic("topic")
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

    [Fact]
    public async Task throws_when_task_is_canceled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var handlerMock = new Mock<IMessageHandler<FooMessage>>();
        var handlerStub = handlerMock.Object;

        var messageRegistrationStub = new MessageRegistrationBuilder()
            .WithHandlerInstanceType(handlerStub.GetType())
            .WithMessageInstanceType(typeof(FooMessage))
            .WithMessageType("foo")
            .WithTopic("")
            .Build();

        var registry = new MessageHandlerRegistry();
        registry.Register(messageRegistrationStub);

        var sut = new ConsumerBuilder()
            .WithUnitOfWork(new UnitOfWorkStub(handlerStub))
            .WithMessageHandlerRegistry(registry)
            .Build();

        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.ConsumeSingle(cts.Token));
    }

    [Fact]
    public async Task dead_letters_and_commits_when_handler_keeps_failing()
    {
        var handlerInvocations = 0;
        var handler = new MessageHandlerSpy<FooMessage>(() =>
        {
            handlerInvocations++;
            throw new InvalidOperationException("boom");
        });

        var deadLetterQueueSpy = new DeadLetterQueueSpy();
        var committed = false;

        var sut = BuildConsumerWithHandler(
            handler,
            onCommit: _ =>
            {
                committed = true;
                return Task.CompletedTask;
            },
            deadLetterQueue: deadLetterQueueSpy,
            maxRetries: 2);

        await sut.ConsumeSingle(CancellationToken.None);

        Assert.Equal(3, handlerInvocations);
        Assert.Equal(1, deadLetterQueueSpy.SendCount);
        Assert.True(committed);
    }

    [Fact]
    public async Task does_not_dead_letter_when_handler_eventually_succeeds()
    {
        var handlerInvocations = 0;
        var handler = new MessageHandlerSpy<FooMessage>(() =>
        {
            handlerInvocations++;
            if (handlerInvocations < 2)
            {
                throw new InvalidOperationException("boom");
            }
        });

        var deadLetterQueueSpy = new DeadLetterQueueSpy();

        var sut = BuildConsumerWithHandler(
            handler,
            deadLetterQueue: deadLetterQueueSpy,
            maxRetries: 2);

        await sut.ConsumeSingle(CancellationToken.None);

        Assert.Equal(2, handlerInvocations);
        Assert.Equal(0, deadLetterQueueSpy.SendCount);
    }

    [Fact]
    public async Task propagates_exception_when_no_dead_letter_queue_is_configured()
    {
        var handler = new MessageHandlerSpy<FooMessage>(() => throw new InvalidOperationException("boom"));

        var sut = BuildConsumerWithHandler(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ConsumeSingle(CancellationToken.None));
    }

    [Fact]
    public async Task does_not_dead_letter_when_cancelled_during_dispatch()
    {
        using var cts = new CancellationTokenSource();

        var handler = new MessageHandlerSpy<FooMessage>(() =>
        {
            cts.Cancel();
            cts.Token.ThrowIfCancellationRequested();
        });

        var deadLetterQueueSpy = new DeadLetterQueueSpy();

        var sut = BuildConsumerWithHandler(
            handler,
            deadLetterQueue: deadLetterQueueSpy,
            maxRetries: 3);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => sut.ConsumeSingle(cts.Token));

        Assert.Equal(0, deadLetterQueueSpy.SendCount);
    }

    private static Consumer BuildConsumerWithHandler(
        IMessageHandler<FooMessage> handler,
        Func<CancellationToken, Task> onCommit = null,
        IDeadLetterQueue deadLetterQueue = null,
        int maxRetries = 0)
    {
        var registration = new MessageRegistrationBuilder()
            .WithHandlerInstanceType(handler.GetType())
            .WithMessageInstanceType(typeof(FooMessage))
            .WithMessageType("foo")
            .WithTopic("topic")
            .Build();

        var registry = new MessageHandlerRegistry();
        registry.Register(registration);

        var messageResult = new MessageResultBuilder()
            .WithTransportLevelMessage(new TransportLevelMessageBuilder().WithType("foo").Build())
            .WithTopic("topic")
            .WithOnCommit(onCommit ?? (_ => Task.CompletedTask))
            .Build();

        var builder = new ConsumerBuilder()
            .WithConsumerScopeFactory(new ConsumerScopeFactoryStub(new ConsumerScopeStub(messageResult)))
            .WithUnitOfWork(new UnitOfWorkStub(handler))
            .WithMessageHandlerRegistry(registry)
            .WithMaxRetries(maxRetries);

        if (deadLetterQueue != null)
        {
            builder.WithDeadLetterQueue(deadLetterQueue);
        }

        return builder.Build();
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

    private class DeadLetterQueueSpy : IDeadLetterQueue
    {
        public int SendCount { get; private set; }
        public MessageResult LastMessage { get; private set; }
        public Exception LastException { get; private set; }

        public Task Send(MessageResult message, Exception exception, CancellationToken cancellationToken)
        {
            SendCount++;
            LastMessage = message;
            LastException = exception;
            return Task.CompletedTask;
        }
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
        cancellationToken.ThrowIfCancellationRequested();
        _onGetNext?.Invoke();

        return Task.FromResult(_messageResult);
    }

    public override void Dispose()
    {
        Disposed++;
    }

    public int Disposed { get; private set; }
}