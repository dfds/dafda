using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    /// <summary>
    /// Implementations should store the state of the Unit of Work
    /// and return the unpublished <see cref="OutboxEntry"/> list using
    /// <see cref="GetAllUnpublishedEntries"/> as part of the same transaction,
    /// which in turn is finished when Dafda calls <see cref="Commit"/>
    /// </summary>
    public interface IOutboxUnitOfWork : IDisposable
    {
        /// <summary>
        /// Returns all unpublished outbox entries from the underlying persistence storage.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token</param>
        /// <returns>The list of unpublished outbox entries</returns>
        Task<ICollection<OutboxEntry>> GetAllUnpublishedEntries(CancellationToken stoppingToken);

        /// <summary>
        /// Commits the Unit of Work; taking care of updating any <see cref="OutboxEntry"/>
        /// that has been processed (using a call to <see cref="OutboxEntry.MaskAsProcessed"/>).
        /// </summary>
        /// <param name="stoppingToken">The cancellation token</param>
        Task Commit(CancellationToken stoppingToken);
    }
}