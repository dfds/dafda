using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;

namespace Dafda.Tests.TestDoubles
{
    internal class FakeOutboxPersistence : IOutboxMessageRepository, IOutboxUnitOfWorkFactory
    {
        public List<OutboxMessage> OutboxMessages { get; }

        public FakeOutboxPersistence(params OutboxMessage[] outboxMessages)
        {
            OutboxMessages = outboxMessages.ToList();
        }

        public Task Add(IEnumerable<OutboxMessage> outboxMessages)
        {
            OutboxMessages.AddRange(outboxMessages);
            return Task.CompletedTask;
        }

        public IOutboxUnitOfWork Begin()
        {
            return new FakeUnitOfWork(this);
        }

        public bool Committed { get; private set; }

        private class FakeUnitOfWork : IOutboxUnitOfWork
        {
            private readonly FakeOutboxPersistence _fake;

            public FakeUnitOfWork(FakeOutboxPersistence fake)
            {
                _fake = fake;
            }

            public Task<ICollection<OutboxMessage>> GetAllUnpublishedMessages(CancellationToken stoppingToken)
            {
                return Task.FromResult<ICollection<OutboxMessage>>(_fake.OutboxMessages.Where(x => x.ProcessedUtc == null).ToList());
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