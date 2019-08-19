using System;
using System.Threading.Tasks;
using Dafda.Messaging;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Moq;
using Xunit;

namespace Dafda.Tests.Messaging
{
    public class TestLocalMessageDispatcher
    {
        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_no_handler_has_been_registered()
        {
            var transportMessageStub = new TransportLevelMessageBuilder().Build();
            var emptyMessageHandlerRegistryStub = new MessageHandlerRegistryStub();
            var dummyFactory = new HandlerUnitOfWorkFactoryStub(null);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(emptyMessageHandlerRegistryStub)
                .WithHandlerUnitOfWorkFactory(dummyFactory)
                .Build();

            await Assert.ThrowsAsync<MissingMessageHandlerRegistrationException>(() => sut.Dispatch(transportMessageStub));
        }

        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_unable_to_resolve_handler_instance()
        {
            var transportMessageStub = new TransportLevelMessageBuilder().Build();
            var messageRegistrationStub = new MessageRegistrationBuilder().Build();

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(new MessageHandlerRegistryStub(messageRegistrationStub))
                .WithHandlerUnitOfWorkFactory(new HandlerUnitOfWorkFactoryStub(null))
                .Build();

            await Assert.ThrowsAsync<UnableToResolveUnitOfWorkForHandlerException>(() => sut.Dispatch(transportMessageStub));
        }

        [Fact]
        public async Task handler_is_invoked_as_expected_when_dispatching()
        {
            var mock = new Mock<IMessageHandler<object>>();

            var transportMessageDummy = new TransportLevelMessageBuilder().Build();
            var registrationDummy = new MessageRegistrationBuilder().Build();

            var typeResolverStub = new TypeResolverStub(mock.Object);

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(new MessageHandlerRegistryStub(registrationDummy))
                .WithHandlerUnitOfWorkFactory(new DefaultUnitOfWorkFactory(typeResolverStub))
                .Build();

            await sut.Dispatch(transportMessageDummy);

            mock.Verify(x => x.Handle(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task handler_exceptions_are_thrown_as_expected()
        {
            var transportMessageDummy = new TransportLevelMessageBuilder().Build();
            var registrationDummy = new MessageRegistrationBuilder().Build();

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(new MessageHandlerRegistryStub(registrationDummy))
                .WithHandlerUnitOfWorkFactory(new DefaultUnitOfWorkFactory(new TypeResolverStub(new ErroneusHandler())))
                .Build();

            await Assert.ThrowsAsync<ExpectedException>(() => sut.Dispatch(transportMessageDummy));
        }

        #region private helper classes

        private class ExpectedException : Exception
        {

        }

        private class ErroneusHandler : IMessageHandler<object>
        {
            public Task Handle(object message)
            {
                throw new ExpectedException();
            }
        }

        #endregion
    }
}