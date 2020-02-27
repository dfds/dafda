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
            var spy = new KafkaProducerSpy();
            var sut = A.OutboxDispatcher
                .With(new FakeOutboxPersistence(A.OutboxMessage
                    .WithTopic("foo")
                    .WithKey("bar")
                    .WithValue("baz")
                ))
                .With(A.OutboxProducer
                    .With(spy)
                    .Build()
                )
                .Build();

            await sut.Dispatch(CancellationToken.None);

            Assert.Equal("foo", spy.Topic);
            Assert.Equal("bar", spy.Key);
            Assert.Equal("baz", spy.Value);
        }
    }
}