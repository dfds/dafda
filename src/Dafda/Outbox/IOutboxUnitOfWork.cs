using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    public interface IOutboxUnitOfWork : IDisposable
    {
        Task<ICollection<OutboxEntry>> GetAllUnpublishedEntries(CancellationToken stoppingToken);
        Task Commit(CancellationToken stoppingToken);
    }
}