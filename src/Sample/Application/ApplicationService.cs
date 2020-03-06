using System.Threading.Tasks;
using Sample.Infrastructure.Persistence;

namespace Sample
{
    public class ApplicationService
    {
        private readonly SampleDbContext _dbContext;
        private readonly DomainEvents _domainEvents;

        public ApplicationService(SampleDbContext dbContext, DomainEvents domainEvents)
        {
            _dbContext = dbContext;
            _domainEvents = domainEvents;
        }

        public Task Process()
        {
            _domainEvents.Raise(new TestEvent {AggregateId = "aggregate-id"});

            return Task.CompletedTask;
        }
    }
}