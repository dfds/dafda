using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    /// <summary>
    /// Any implementation of the <see cref="IOutboxEntryRepository"/> must
    /// take care of storing the outboxEntries received in the <see cref="Add"/>
    /// method implementation, as part of the database transaction that also
    /// persists the business/domain entities.
    /// </summary>
    public interface IOutboxEntryRepository
    {
        /// <summary>
        /// Add one or more instances of the <see cref="OutboxEntry"/> to
        /// be dispatched by the Dafda outbox
        /// </summary>
        /// <param name="outboxEntries">The outbox entries.</param>
        Task Add(IEnumerable<OutboxEntry> outboxEntries);
    }
}