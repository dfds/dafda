﻿using System.Threading.Tasks;
using Dafda.Tests.Helpers;
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
            var sut = A.Producer
                .With(spy)
                .With(A.OutgoingMessageRegistry
                    .Register<DummyMessage>(DummyTopic, DummyType, x => x.AggregateId)
                    .Build()
                )
                .With(new MessageIdGeneratorStub(() => dummyMessageId))
                .Build();

            await sut.Produce(new DummyMessage(dummyAggregateId));

            var expected = $@"{{
                                ""messageId"":""{dummyMessageId}"",
                                ""type"":""{DummyType}"",
                                ""causationId"":""foo_id"",
                                ""correlationId"":""foo_id"",
                                ""data"":{{
                                    ""aggregateId"":""{dummyAggregateId}""
                                    }}
                                }}";

            AssertJson.Equal(expected, spy.Value);
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