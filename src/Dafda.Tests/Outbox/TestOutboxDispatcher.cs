using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Outbox
{
    public class TestOutboxDispatcher
    {
        [Fact]
        public async Task Can_processes_unpublished_outbox_messages()
        {
            var dummyMessageId = Guid.NewGuid();
            var spy = new KafkaProducerSpy();
            var sut = A.OutboxDispatcher
                .With(new FakeOutboxPersistence(A.OutboxMessage
                    .WithMessageId(dummyMessageId)
                    .WithTopic("foo")
                    .WithKey("bar")
                    .WithType("baz")
                    .WithValue("qux")
                    .OccurredOnUtc(DateTime.UtcNow)
                ))
                .With(A.OutboxProducer
                    .With(spy)
                    .Build()
                )
                .Build();

            await sut.Dispatch(CancellationToken.None);

            Assert.Equal(dummyMessageId.ToString(), spy.LastMessage.MessageId);
            Assert.Equal("foo", spy.LastMessage.Topic);
            Assert.Equal("bar", spy.LastMessage.Key);
            Assert.Equal("baz", spy.LastMessage.Type);
            Assert.Equal("qux", spy.LastMessage.Value);
        }
    }
}