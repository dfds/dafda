using System.Threading.Tasks;
using Dafda.Producing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestOutgoingMessageFactory
    {
        private const string DummyTopic = "dummy_topic";
        private const string DummyType = "dummy_type";

        [Fact]
        public async Task Can_create_outgoing_message_from_registry_with_expected_raw_message()
        {
            const string dummyMessageId = "foo_id";
            const string dummyAggregateId = "dummyId";

            var spy = new KafkaProducerSpy();
            var outgoingMessageRegistry = new OutgoingMessageRegistryBuilder().Register<DummyMessage>(DummyTopic, DummyType, x => x.AggregateId).Build();
            var sut = new Producer(spy, outgoingMessageRegistry, new MessageIdGeneratorStub(() => dummyMessageId));

            await sut.Produce(new DummyMessage(dummyAggregateId));

            Assert.Equal($@"{{""messageId"":""{dummyMessageId}"",""type"":""{DummyType}"",""data"":{{""aggregateId"":""{dummyAggregateId}""}}}}", spy.LastMessage.Value);
        }

        private class DummyMessage
        {
            public DummyMessage(string aggregateId = null)
            {
                AggregateId = aggregateId;
            }

            public string AggregateId { get; }
        }
    }
}