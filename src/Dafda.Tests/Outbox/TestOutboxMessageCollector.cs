using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Outbox
{
    public class TestOutboxMessageCollector
    {
        [Fact]
        public async Task Fails_for_unregistered_outgoing_messages()
        {
            var sut = new OutboxMessageCollector(MessageIdGenerator.Default, new OutgoingMessageRegistryBuilder().Build(), new OutboxMessageRespositorySpy());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Enqueue(new[] {new DomainEvent()}));
        }

        [Fact]
        public async Task Can_delegate_persistence_for_outgoing_message()
        {
            var stub = new MessageIdGeneratorStub(() => Guid.Empty.ToString());
            var registry = new OutgoingMessageRegistryBuilder()
                .Register<DomainEvent>("foo", "bar", @event => "baz")
                .Build();
            var spy = new OutboxMessageRespositorySpy();
            var sut = new OutboxMessageCollector(stub, registry, spy);

            await sut.Enqueue(new[] {new DomainEvent()});

            Assert.NotEmpty(spy.OutboxMessages);
        }
    }

    public class DomainEvent
    {
    }

    public class OutboxMessageRespositorySpy : IOutboxMessageRepository
    {
        public List<OutboxMessage> OutboxMessages { get; } = new List<OutboxMessage>();

        public Task Add(IEnumerable<OutboxMessage> outboxMessages)
        {
            OutboxMessages.AddRange(outboxMessages);
            return Task.CompletedTask;
        }
    }
}