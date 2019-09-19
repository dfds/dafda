using Dafda.Producing;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestOutgoingMessage
    {
        [Fact]
        public void Has_expected_message_id_header_name()
        {
            Assert.Equal("messageId", OutgoingMessage.MessageIdHeaderName);
        }

        [Fact]
        public void Has_expected_type_header_name()
        {
            Assert.Equal("type", OutgoingMessage.TypeHeaderName);
        }
    }
}