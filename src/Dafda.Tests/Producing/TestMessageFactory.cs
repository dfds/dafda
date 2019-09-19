using Dafda.Producing;
using Dafda.Tests.Builders;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestMessageFactory
    {
        [Fact]
        public void Message_has_expected_key()
        {
            var message = MessageFactory.Create(new OutgoingMessageBuilder().WithKey("dummyKey"));

            Assert.Equal("dummyKey", message.Key);
        }

        [Fact]
        public void Message_has_expected_value()
        {
            var message = MessageFactory.Create(new OutgoingMessageBuilder().WithRawMessage("dummyMessage"));

            Assert.Equal("dummyMessage", message.Value);
        }

        [Fact]
        public void Message_header_has_expected_message_id()
        {
            var message = MessageFactory.Create(new OutgoingMessageBuilder().WithMessageId("A"));

            Assert.Equal(new[] {(byte) 'A'}, message.Headers.GetLastBytes(OutgoingMessage.MessageIdHeaderName));
        }

        [Fact]
        public void Message_header_has_expected_type()
        {
            var message = MessageFactory.Create(new OutgoingMessageBuilder().WithType("T"));

            Assert.Equal(new[] {(byte) 'T'}, message.Headers.GetLastBytes(OutgoingMessage.TypeHeaderName));
        }
    }
}