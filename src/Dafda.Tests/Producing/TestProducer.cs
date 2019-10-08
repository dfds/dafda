using System.Threading.Tasks;
using Dafda.Producing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestProducer
    {
        [Fact]
        public async Task Can_produce_message()
        {
            var spy = new KafkaProducerSpy();
            var messageRegistry = new OutgoingMessageRegistryBuilder()
                .Register<DomainEvent>("foo", "bar", @event => @event.AggregateId)
                .Build();

            var sut = new Producer(spy, messageRegistry, MessageIdGenerator.Default);

            await sut.Produce(new DomainEvent
            {
                AggregateId = "dummyId"
            });

            Assert.Equal("foo", spy.LastMessage.Topic);
            Assert.Equal("dummyId", spy.LastMessage.Key);
        }

        public class DomainEvent
        {
            public string AggregateId { get; set; }
        }
    }
}