using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Outbox;

namespace Dafda.Tests.TestDoubles
{
    public class OutboxEntryRepositorySpy : IOutboxEntryRepository
    {
        public List<OutboxEntry> OutboxEntries { get; } = new List<OutboxEntry>();

        public Task Add(IEnumerable<OutboxEntry> outboxEntries)
        {
            OutboxEntries.AddRange(outboxEntries);
            return Task.CompletedTask;
        }
    }
}