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
        public void Can_create_outgoing_message_from_metadata_with_expected_topic()
        {
            var sut = new OutgoingMessageFactoryBuilder().Build();

            var outgoingMessage = sut.Create(new DummyMessageWithMetaData());

            Assert.Equal(DummyTopic, outgoingMessage.Topic);
        }

        [Fact]
        public void Can_create_outgoing_message_from_metadata_with_expected_message_id()
        {
            const string dummyMessageId = "foo";

            var sut = new OutgoingMessageFactoryBuilder()
                .With(new MessageIdGeneratorStub(() => dummyMessageId))
                .Build();

            var outgoingMessage = sut.Create(new DummyMessageWithMetaData());

            Assert.Equal(dummyMessageId, outgoingMessage.MessageId);
        }

        [Fact]
        public void Can_create_outgoing_message_from_metadata_with_expected_key()
        {
            const string dummyAggregateId = "dummyId";

            var sut = new OutgoingMessageFactoryBuilder().Build();

            var outgoingMessage = sut.Create(new DummyMessageWithMetaData(dummyAggregateId));

            Assert.Equal(dummyAggregateId, outgoingMessage.Key);
        }

        [Fact]
        public void Can_create_outgoing_message_from_metadata_with_expected_type()
        {
            var sut = new OutgoingMessageFactoryBuilder().Build();

            var outgoingMessage = sut.Create(new DummyMessageWithMetaData());

            Assert.Equal(DummyType, outgoingMessage.Type);
        }

        [Fact]
        public void Can_create_outgoing_message_from_metadata_with_expected_raw_message()
        {
            const string dummyMessageId = "foo_id";
            const string dummyAggregateId = "dummyId";

            var sut = new OutgoingMessageFactoryBuilder()
                .With(new MessageIdGeneratorStub(() => dummyMessageId))
                .Build();

            var outgoingMessage = sut.Create(new DummyMessageWithMetaData(dummyAggregateId));

            Assert.Equal($@"{{""messageId"":""{dummyMessageId}"",""type"":""{DummyType}"",""data"":{{""aggregateId"":""{dummyAggregateId}""}}}}", outgoingMessage.Value);
        }

        [Fact]
        public void Can_create_outgoing_message_from_registry_with_expected_topic()
        {
            var sut = new OutgoingMessageFactoryBuilder()
                .With(new OutgoingMessageRegistryBuilder().Register<DummyMessage>(DummyTopic, DummyType, x => x.AggregateId))
                .Build();

            var outgoingMessage = sut.Create(new DummyMessage());

            Assert.Equal(DummyTopic, outgoingMessage.Topic);
        }

        [Fact]
        public void Can_create_outgoing_message_from_registry_with_expected_message_id()
        {
            const string dummyMessageId = "foo";

            var sut = new OutgoingMessageFactoryBuilder()
                .With(new MessageIdGeneratorStub(() => dummyMessageId))
                .With(new OutgoingMessageRegistryBuilder().Register<DummyMessage>(DummyTopic, DummyType, x => x.AggregateId))
                .Build();

            var outgoingMessage = sut.Create(new DummyMessage());

            Assert.Equal(dummyMessageId, outgoingMessage.MessageId);
        }

        [Fact]
        public void Can_create_outgoing_message_from_registry_with_expected_key()
        {
            const string dummyAggregateId = "dummyId";

            var sut = new OutgoingMessageFactoryBuilder()
                .With(new OutgoingMessageRegistryBuilder().Register<DummyMessage>(DummyTopic, DummyType, x => x.AggregateId))
                .Build();

            var outgoingMessage = sut.Create(new DummyMessage(dummyAggregateId));

            Assert.Equal(dummyAggregateId, outgoingMessage.Key);
        }

        [Fact]
        public void Can_create_outgoing_message_from_registry_with_expected_type()
        {
            var sut = new OutgoingMessageFactoryBuilder()
                .With(new OutgoingMessageRegistryBuilder().Register<DummyMessage>(DummyTopic, DummyType, x => x.AggregateId))
                .Build();

            var outgoingMessage = sut.Create(new DummyMessage());

            Assert.Equal(DummyType, outgoingMessage.Type);
        }

        [Fact]
        public void Can_create_outgoing_message_from_registry_with_expected_raw_message()
        {
            const string dummyMessageId = "foo_id";
            const string dummyAggregateId = "dummyId";

            var sut = new OutgoingMessageFactoryBuilder()
                .With(new MessageIdGeneratorStub(() => dummyMessageId))
                .With(new OutgoingMessageRegistryBuilder().Register<DummyMessage>(DummyTopic, DummyType, x => x.AggregateId))
                .Build();

            var outgoingMessage = sut.Create(new DummyMessage(dummyAggregateId));

            Assert.Equal($@"{{""messageId"":""{dummyMessageId}"",""type"":""{DummyType}"",""data"":{{""aggregateId"":""{dummyAggregateId}""}}}}", outgoingMessage.Value);
        }

        [Message(DummyTopic, DummyType)]
        private class DummyMessageWithMetaData : IMessage
        {
            public DummyMessageWithMetaData(string aggregateId = null)
            {
                AggregateId = aggregateId;
            }

            public string AggregateId { get; }
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