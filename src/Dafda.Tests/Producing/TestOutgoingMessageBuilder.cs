using Dafda.Producing;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestOutgoingMessageBuilder
    {
        private const string DummyTopic = "dummy_topic";
        private const string DummyType = "dummy_type";

        [Fact]
        public void Can_create_outgoing_message_with_expected_topic()
        {
            var outgoingMessage = new OutgoingMessageBuilder()
                .WithTopic(DummyTopic)
                .Build();

            Assert.Equal(DummyTopic, outgoingMessage.Topic);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_message_id()
        {
            const string dummyMessageId = "foo";

            var outgoingMessage = new OutgoingMessageBuilder()
                .WithMessageId(dummyMessageId)
                .Build();

            Assert.Equal(dummyMessageId, outgoingMessage.MessageId);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_key()
        {
            const string dummyAggregateId = "dummyId";

            var outgoingMessage = new OutgoingMessageBuilder()
                .WithKey(dummyAggregateId)
                .Build();

            Assert.Equal(dummyAggregateId, outgoingMessage.Key);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_type()
        {
            var outgoingMessage = new OutgoingMessageBuilder()
                .WithType(DummyType)
                .Build();

            Assert.Equal(DummyType, outgoingMessage.Type);
        }

        [Fact]
        public void Can_create_outgoing_message_with_expected_raw_message()
        {
            var outgoingMessage = new OutgoingMessageBuilder()
                .WithValue("rawMessage")
                .Build();

            Assert.Equal("rawMessage", outgoingMessage.Value);
        }
    }
}