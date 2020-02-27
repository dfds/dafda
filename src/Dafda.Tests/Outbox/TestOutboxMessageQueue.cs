using System;
using System.Threading.Tasks;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Outbox
{
    public class TestOutboxMessageQueue
    {
        [Fact]
        public async Task Fails_for_unregistered_outgoing_messages()
        {
            var sut = A.OutboxQueue.Build();

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Enqueue(new[] {new Message()}));
        }

        [Fact]
        public async Task Can_delegate_persistence_for_outgoing_message()
        {
            var spy = new OutboxMessageRepositorySpy();

            var sut = A.OutboxQueue
                .With(
                    A.OutgoingMessageRegistry
                        .Register<Message>("foo", "bar", @event => "baz")
                        .Build()
                )
                .With(spy)
                .Build();

            await sut.Enqueue(new[] {new Message()});

            Assert.NotEmpty(spy.OutboxMessages);
        }

        public class Message
        {
        }
    }
}