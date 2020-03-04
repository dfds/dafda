using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Outbox;

namespace Outbox.Infrastructure.Persistence
{
    public class OutboxEntryRepository : IOutboxEntryRepository
    {
        private readonly SampleDbContext _dbContext;

        public OutboxEntryRepository(SampleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(IEnumerable<OutboxEntry> outboxEntries)
        {
            await _dbContext.OutboxEntries.AddRangeAsync(outboxEntries);
        }
    }
}