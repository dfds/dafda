using System.Threading.Tasks;
using Dafda.Producing;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestBus
    {
        [Fact]
        public async Task Can_produce_message()
        {
            var spy = new ProducerSpy();

            var sut = new Bus(spy);

            await sut.Publish(new DomainEvent
            {
                AggregateId = "dummyId"
            });

            Assert.Equal("foo", spy.LastOutgoingMessage.Topic);
            Assert.Equal("dummyId", spy.LastOutgoingMessage.Key);
        }

        [Message("foo", "bar")]
        public class DomainEvent : IMessage
        {
            public string AggregateId { get; set; }
        }
    }
}