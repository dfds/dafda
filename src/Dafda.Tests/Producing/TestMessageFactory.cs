using Dafda.Producing;
using Dafda.Tests.Builders;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestMessageFactory
    {
        [Fact]
        public void Has_expected_message_id_header_name()
        {
            Assert.Equal("messageId", MessageFactory.MessageIdHeaderName);
        }

        [Fact]
        public void Has_expected_type_header_name()
        {
            Assert.Equal("type", MessageFactory.TypeHeaderName);
        }

        [Fact]
        public void Message_has_expected_key()
        {
            var message = MessageFactory.Create(new OutgoingMessageBuilder().WithKey("dummyKey"));

            Assert.Equal("dummyKey", message.Key);
        }

        [Fact]
        public void Message_has_expected_value()
        {
            var message = MessageFactory.Create(new OutgoingMessageBuilder().WithValue("dummyMessage"));

            Assert.Equal("dummyMessage", message.Value);
        }

        [Fact]
        public void Message_header_has_expected_message_id()
        {
            var message = MessageFactory.Create(new OutgoingMessageBuilder().WithMessageId("A"));

            Assert.Equal(new[] {(byte) 'A'}, message.Headers.GetLastBytes(MessageFactory.MessageIdHeaderName));
        }

        [Fact]
        public void Message_header_has_expected_type()
        {
            var message = MessageFactory.Create(new OutgoingMessageBuilder().WithType("T"));

            Assert.Equal(new[] {(byte) 'T'}, message.Headers.GetLastBytes(MessageFactory.TypeHeaderName));
        }
    }
}