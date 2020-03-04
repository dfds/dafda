using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;

namespace Dafda.Tests.TestDoubles
{
    internal class FakeOutboxPersistence : IOutboxEntryRepository, IOutboxUnitOfWorkFactory
    {
        public List<OutboxEntry> OutboxEntries { get; }

        public FakeOutboxPersistence(params OutboxEntry[] outboxEntries)
        {
            OutboxEntries = outboxEntries.ToList();
        }

        public Task Add(IEnumerable<OutboxEntry> outboxEntries)
        {
            OutboxEntries.AddRange(outboxEntries);
            return Task.CompletedTask;
        }

        public Task<IOutboxUnitOfWork> Begin(CancellationToken cancellationToken)
        {
            return Task.FromResult<IOutboxUnitOfWork>(new FakeUnitOfWork(this));
        }

        public bool Committed { get; private set; }

        private class FakeUnitOfWork : IOutboxUnitOfWork
        {
            private readonly FakeOutboxPersistence _fake;

            public FakeUnitOfWork(FakeOutboxPersistence fake)
            {
                _fake = fake;
            }

            public Task<ICollection<OutboxEntry>> GetAllUnpublishedEntries(CancellationToken stoppingToken)
            {
                return Task.FromResult<ICollection<OutboxEntry>>(_fake.OutboxEntries.Where(x => x.ProcessedUtc == null).ToList());
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