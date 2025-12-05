using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Moq;
using Xunit;

namespace Dafda.Tests.Consuming
{
    public class TestLocalMessageDispatcher
    {
        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_no_handler_has_been_registered()
        {
            using var cts = new CancellationTokenSource();
            var messageResultStub = new MessageResultBuilder().Build();
            var emptyMessageHandlerRegistryStub = new MessageHandlerRegistry();
            var dummyFactory = new HandlerUnitOfWorkFactoryStub(null);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(emptyMessageHandlerRegistryStub)
                .WithHandlerUnitOfWorkFactory(dummyFactory)
                .Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(() => sut.Dispatch(messageResultStub, cts.Token));
        }

        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_unable_to_resolve_handler_instance()
        {
            using var cts = new CancellationTokenSource();
            var transportMessageDummy = new TransportLevelMessageBuilder().WithType("foo").Build();
            var messageResultStub = new MessageResultBuilder().WithTopic("topic").WithTransportLevelMessage(transportMessageDummy).Build();
            var messageRegistrationStub = new MessageRegistrationBuilder().WithTopic("topic").WithMessageType("foo").Build();
            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(registry)
                .WithHandlerUnitOfWorkFactory(new HandlerUnitOfWorkFactoryStub(null))
                .Build();

            await Assert.ThrowsAsync<UnableToResolveUnitOfWorkForHandlerException>(() => sut.Dispatch(messageResultStub, cts.Token));
        }

        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_unable_to_resolve_handler_instance2()
        {
            using var cts = new CancellationTokenSource();
            var transportMessageDummy = new TransportLevelMessageBuilder().WithType("foo").Build();
            var messageResultStub = new MessageResultBuilder().WithTopic("topic").WithTransportLevelMessage(transportMessageDummy).Build();
            var messageRegistrationStub = new MessageRegistrationBuilder().WithTopic("topic-other").WithMessageType("foo").Build();
            var registry = new MessageHandlerRegistry();
            registry.Register(messageRegistrationStub);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(registry)
                .WithHandlerUnitOfWorkFactory(new HandlerUnitOfWorkFactoryStub(null))
                .Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(() => sut.Dispatch(messageResultStub, cts.Token));
        }

        [Fact]
        public async Task handler_is_invoked_as_expected_when_dispatching()
        {
            using var cts = new CancellationTokenSource();
            var mock = new Mock<IMessageHandler<object>>();

            var transportMessageDummy = new TransportLevelMessageBuilder().WithType("foo").Build();
            var messageResultStub = new MessageResultBuilder().WithTopic("topic").WithTransportLevelMessage(transportMessageDummy).Build();
            var registrationDummy = new MessageRegistrationBuilder().WithMessageType("foo").WithTopic("topic").Build();
            var registry = new MessageHandlerRegistry();
            registry.Register(registrationDummy);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(registry)
                .WithHandlerUnitOfWork(new UnitOfWorkStub(mock.Object))
                .Build();

            await sut.Dispatch(messageResultStub, cts.Token);

            mock.Verify(x => x.Handle(It.IsAny<object>(), It.IsAny<MessageHandlerContext>(), cts.Token), Times.Once);
        }

        [Fact]
        public async Task handler_exceptions_are_thrown_as_expected()
        {
            using var cts = new CancellationTokenSource();
            var transportMessageDummy = new TransportLevelMessageBuilder().WithType("foo").Build();
            var messageResultStub = new MessageResultBuilder().WithTopic("topic").WithTransportLevelMessage(transportMessageDummy).Build();
            var registrationDummy = new MessageRegistrationBuilder().WithTopic("topic").WithMessageType("foo").Build();
            var registry = new MessageHandlerRegistry();
            registry.Register(registrationDummy);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(registry)
                .WithHandlerUnitOfWork(new UnitOfWorkStub(new ErroneusHandler()))
                .Build();

            await Assert.ThrowsAsync<ExpectedException>(() => sut.Dispatch(messageResultStub, cts.Token));
        }

        [Fact]
        public async Task handler_is_executed_in_consumer_execution_strategy()
        {
            using var cts = new CancellationTokenSource();
            var mock = new Mock<IMessageHandler<object>>();
            var consumerExecutionStrategyMock = new Mock<IConsumerExecutionStrategy>();

            var transportMessageDummy = new TransportLevelMessageBuilder().WithType("foo").Build();
            var messageResultStub = new MessageResultBuilder().WithTopic("topic").WithTransportLevelMessage(transportMessageDummy).Build();
            var registrationDummy = new MessageRegistrationBuilder().WithMessageType("foo").WithTopic("topic").Build();
            var registry = new MessageHandlerRegistry();
            registry.Register(registrationDummy);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(registry)
                .WithHandlerUnitOfWork(new UnitOfWorkStub(mock.Object))
                .WithConsumerExecutionStrategy(consumerExecutionStrategyMock.Object)
                .Build();

            await sut.Dispatch(messageResultStub, cts.Token);

            consumerExecutionStrategyMock.Verify(x => x.Execute(It.IsAny<Func<Task>>()), Times.Once);
        }

        #region private helper classes

        private class ExpectedException : Exception
        {

        }

        private class ErroneusHandler : IMessageHandler<object>
        {
            public Task Handle(object message, MessageHandlerContext context, CancellationToken cancellationToken = default)
            {
                throw new ExpectedException();
            }
        }

        #endregion
    }
}