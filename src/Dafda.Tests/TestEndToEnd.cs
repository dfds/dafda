using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;
using Dafda.Messaging;
using Dafda.Tests.Messaging;
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
                .WithUnitOfWorkFactory(type => new DirectUnitOfWork(mock.Object))
                .RegisterMessageHandler<FooMessage, IMessageHandler<FooMessage>>("dummy-topic", "foo")
                .WithInternalConsumerFactory(new InternalConsumerFactoryStub(new InternalConsumerStub("foo")))
                .Build();

            var sut = new NewConsumer(configuration);

            await sut.ConsumeSingle(CancellationToken.None);

            mock.Verify(x => x.Handle(It.IsAny<FooMessage>()), Times.Once);
        }

        [Fact]
        public async Task duno2()
        {
            var orderOfInvocation = new LinkedList<string>();

            var spy = new UnitOfWorkSpy(
                handlerInstance: new HandlerSpy<FooMessage>(() => orderOfInvocation.AddLast("during")),
                pre: () => orderOfInvocation.AddLast("before"),
                post: () => orderOfInvocation.AddLast("after")
            );

            var configuration = new ConsumerConfigurationBuilder()
                .WithGroupId("foo")
                .WithBootstrapServers("bar")
                .WithUnitOfWorkFactory(type => spy)
                .RegisterMessageHandler<FooMessage, IMessageHandler<FooMessage>>("dummy-topic", "foo")
                .WithInternalConsumerFactory(new InternalConsumerFactoryStub(new InternalConsumerStub("foo")))
                .Build();

            var sut = new NewConsumer(configuration);

            await sut.ConsumeSingle(CancellationToken.None);

            Assert.Equal(new[]{"before", "during", "after"}, orderOfInvocation);
        }

        #region private helper classes

        public class HandlerSpy<TMessage> : IMessageHandler<TMessage> where TMessage : class, new()
        {
            private readonly Action _onHandle;

            public HandlerSpy(Action onHandle)
            {
                _onHandle = onHandle;
            }
            
            public Task Handle(TMessage message)
            {
                _onHandle?.Invoke();
                return Task.CompletedTask;
            }
        }

        public class UnitOfWorkSpy : IHandlerUnitOfWork
        {
            private readonly object _handlerInstance;
            private readonly Action _pre;
            private readonly Action _post;

            public UnitOfWorkSpy(object handlerInstance, Action pre = null, Action post = null)
            {
                _handlerInstance = handlerInstance;
                _pre = pre;
                _post = post;
            }
            
            public async Task Run(Func<object, Task> handlingAction)
            {
                _pre?.Invoke();
                await handlingAction(_handlerInstance);
                _post?.Invoke();
            }
        }

        public class FooMessage
        {
            public string Value { get; set; }
        }

        #endregion
    }

    public class InternalConsumerFactoryStub : IInternalConsumerFactory
    {
        private readonly IInternalConsumer _result;

        public InternalConsumerFactoryStub(IInternalConsumer result)
        {
            _result = result;
        }
            
        public IInternalConsumer CreateConsumer(IConsumerConfiguration configuration)
        {
            return _result;
        }
    }
}