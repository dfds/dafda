using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    public interface IOutboxEntryRepository
    {
        Task Add(IEnumerable<OutboxEntry> outboxEntries);
    }
}