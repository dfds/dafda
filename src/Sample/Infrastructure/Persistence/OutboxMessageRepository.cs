using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Outbox;

namespace Sample.Infrastructure.Persistence
{
    public class OutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly SampleDbContext _dbContext;

        public OutboxMessageRepository(SampleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(IEnumerable<OutboxMessage> domainEvents)
        {
            await _dbContext.OutboxMessages.AddRangeAsync(domainEvents);
        }
    }
}