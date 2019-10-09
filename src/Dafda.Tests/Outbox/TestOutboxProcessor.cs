using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Dafda.Producing;
using Dafda.Tests.Builders;
using Dafda.Tests.TestDoubles;
using Xunit;

namespace Dafda.Tests.Outbox
{
    public class TestOutboxProcessor
    {
        [Fact]
        public async Task Can_processes_unpublished_outbox_messages()
        {
            var stub = new OutgoingMessageRegistryBuilder()
                .Build();
            var messageId = Guid.NewGuid();
            var outboxMessages = new OutboxMessage(messageId, "foo", "bar", "baz", "qux", "dummy", "quux", DateTime.Now);
            var fake = new FakeUnitOfWorkFactory(outboxMessages);
            var spy = new KafkaProducerSpy();
            var sut = new OutboxProcessor(fake, new Producer(spy, stub, MessageIdGenerator.Default));

            await sut.ProcessUnpublishedOutboxMessages(CancellationToken.None);

            Assert.Equal(messageId.ToString(), spy.LastMessage.MessageId);
            Assert.Equal("bar", spy.LastMessage.Topic);
            Assert.Equal("baz", spy.LastMessage.Key);
            Assert.Equal("qux", spy.LastMessage.Type);
            Assert.Equal("quux", spy.LastMessage.Value);
            Assert.True(fake.Committed);
        }
    }

    public class FakeUnitOfWorkFactory : IOutboxUnitOfWorkFactory
    {
        public List<OutboxMessage> OutboxMessages { get; }

        public FakeUnitOfWorkFactory(params OutboxMessage[] outboxMessages)
        {
            OutboxMessages = outboxMessages.ToList();
        }

        public bool Committed { get; private set; }

        public IOutboxUnitOfWork Begin()
        {
            return new _(this);
        }

        private class _ : IOutboxUnitOfWork
        {
            private readonly FakeUnitOfWorkFactory _fake;

            public _(FakeUnitOfWorkFactory fake)
            {
                _fake = fake;
            }

            public Task<ICollection<OutboxMessage>> GetAllUnpublishedMessages(CancellationToken stoppingToken)
            {
                return Task.FromResult<ICollection<OutboxMessage>>(_fake.OutboxMessages.Where(x => !x.ProcessedUtc.HasValue).ToList());
            }

            public Task Commit(CancellationToken stoppingToken)
            {
                _fake.Committed = true;
                return Task.CompletedTask;
            }

            public void Dispose()
            {
            }
        }
    }
}