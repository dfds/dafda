using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    public interface IOutboxUnitOfWork : IDisposable
    {
        Task<ICollection<OutboxMessage>> GetAllUnpublishedMessages(CancellationToken stoppingToken);
        Task Commit(CancellationToken stoppingToken);
    }
}