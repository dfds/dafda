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
            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(new MessageHandlerRegistryStub())
                .Build();

            await Assert.ThrowsAsync<MissingMessageHandlerException>(() => sut.Dispatch(transportMessageStub));
        }

        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_unable_to_resolve_handler_instance()
        {
            var transportMessageStub = new TransportLevelMessageBuilder().Build();
            var messageRegistrationStub = new MessageRegistrationBuilder().Build();

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(new MessageHandlerRegistryStub(messageRegistrationStub))
                .WithTypeResolver(new TypeResolverStub(null))
                .Build();

            await Assert.ThrowsAsync<UnableToResolveMessageHandlerException>(() => sut.Dispatch(transportMessageStub));
        }

        [Fact]
        public async Task handler_is_invoked_as_expected_when_dispatching()
        {
            var mock = new Mock<IMessageHandler<object>>();

            var transportMessageDummy = new TransportLevelMessageBuilder().Build();
            var registrationDummy = new MessageRegistrationBuilder().Build();

            var sut = new LocalMessageDispatcherBuilder()
                .WithTypeResolver(new TypeResolverStub(mock.Object))
                .WithMessageHandlerRegistry(new MessageHandlerRegistryStub(registrationDummy))
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
                .WithTypeResolver(new TypeResolverStub(new ErroneusHandler()))
                .WithMessageHandlerRegistry(new MessageHandlerRegistryStub(registrationDummy))
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