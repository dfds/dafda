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
    }
}