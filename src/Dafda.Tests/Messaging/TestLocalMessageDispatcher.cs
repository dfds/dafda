using System.Threading.Tasks;
using Dafda.Messaging;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Messaging
{
    public class TestLocalMessageDispatcher
    {
        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_no_handler_has_been_registered()
        {
            var messageStub = new TransportLevelMessageStub(type: "foo");
            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(new MessageHandlerRegistryStub())
                .Build();

            await Assert.ThrowsAsync<MissingMessageHandlerException>(() => sut.Dispatch(messageStub));
        }

        [Fact]
        public async Task throws_expected_exception_when_dispatching_and_unable_to_resolve_handler_instance()
        {
            var messageStub = new TransportLevelMessageStub(type: "foo");
            var messageRegistrationStub = new MessageRegistrationBuilder().Build();

            var sut = new LocalMessageDispatcherBuilder()
                .WithMessageHandlerRegistry(new MessageHandlerRegistryStub(messageRegistrationStub))
                .WithTypeResolver(new TypeResolverStub(null))
                .Build();

            await Assert.ThrowsAsync<UnableToResolveMessageHandlerException>(() => sut.Dispatch(messageStub));
        }
    }
}