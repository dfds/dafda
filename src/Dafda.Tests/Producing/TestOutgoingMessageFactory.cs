using Dafda.Producing;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestOutgoingMessageFactory
    {
        private const string DummyTopic = "dummy_topic";
        private const string DummyType = "dummy_type";

        [Fact]
        public void Can_create_outgoing_message_with_expected_topic()
        {
            var sut = new OutgoingMessageFactory();

            var outgoingMessage = sut.Create(new DummyMessage());

            Assert.Equal(DummyTopic, outgoingMessage.Topic);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_message_id()
        {
            const string dummyMessageId = "foo";

            var sut = new OutgoingMessageFactory(new MessageIdGeneratorStub(dummyMessageId));

            var outgoingMessage = sut.Create(new DummyMessage());

            Assert.Equal(dummyMessageId, outgoingMessage.MessageId);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_key()
        {
            const string dummyAggregateId = "dummyId";

            var sut = new OutgoingMessageFactory();

            var outgoingMessage = sut.Create(new DummyMessage(dummyAggregateId));

            Assert.Equal(dummyAggregateId, outgoingMessage.Key);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_type()
        {
            var sut = new OutgoingMessageFactory();

            var outgoingMessage = sut.Create(new DummyMessage());

            Assert.Equal(DummyType, outgoingMessage.Type);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_raw_message()
        {
            const string dummyMessageId = "foo_id";
            const string dummyAggregateId = "dummyId";

            var sut = new OutgoingMessageFactory(new MessageIdGeneratorStub(dummyMessageId));

            var outgoingMessage = sut.Create(new DummyMessage(dummyAggregateId));

            Assert.Equal($@"{{""messageId"":""{dummyMessageId}"",""type"":""{DummyType}"",""data"":{{""aggregateId"":""{dummyAggregateId}""}}}}", outgoingMessage.RawMessage);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_message_id_header()
        {
            const string dummyMessageId = "foo_id";

            var sut = new OutgoingMessageFactory(new MessageIdGeneratorStub(dummyMessageId));

            var outgoingMessage = sut.Create(new DummyMessage());

            Assert.Equal(dummyMessageId, outgoingMessage.Headers[OutgoingMessage.MessageIdHeaderName]);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_type_header()
        {
            var sut = new OutgoingMessageFactory();

            var outgoingMessage = sut.Create(new DummyMessage());

            Assert.Equal(DummyType, outgoingMessage.Headers[OutgoingMessage.TypeHeaderName]);
        }

        [Message(DummyTopic, DummyType)]
        private class DummyMessage : IMessage
        {
            public DummyMessage(string aggregateId = null)
            {
                AggregateId = aggregateId;
            }

            public string AggregateId { get; }
        }
    }
}