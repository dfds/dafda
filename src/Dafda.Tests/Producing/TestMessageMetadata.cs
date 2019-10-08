using System;
using Dafda.Producing;
using Xunit;

namespace Dafda.Tests.Producing
{
    public class TestMessageMetadata
    {
        [Fact]
        public void Can_get_message_metadata_topic()
        {
            var sut = MessageMetadata.Create(new MessageWithMetadata());

            Assert.Equal("foo", sut.TopicName);
        }

        [Fact]
        public void Can_get_message_metadata_type()
        {
            var sut = MessageMetadata.Create(new MessageWithMetadata());

            Assert.Equal("bar", sut.Type);
        }

        [Fact]
        public void Throws_on_missing_message_attribute()
        {
            Assert.Throws<InvalidOperationException>(() => MessageMetadata.Create(new MessageWithoutMetadata()));
        }

        [Message("foo", "bar")]
        public class MessageWithMetadata : IMessage
        {
            public string AggregateId { get; }
        }

        public class MessageWithoutMetadata : IMessage
        {
            public string AggregateId { get; }
        }
    }
}