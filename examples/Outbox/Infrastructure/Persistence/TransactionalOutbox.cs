using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Outbox.Domain;

namespace Outbox.Infrastructure.Persistence
{
    public class TransactionalOutbox
    {
        private readonly SampleDbContext _dbContext;
        private readonly OutboxQueue _outboxQueue;
        private readonly DomainEvents _domainEvents;

        public TransactionalOutbox(SampleDbContext dbContext, OutboxQueue outboxQueue, DomainEvents domainEvents)
        {
            _dbContext = dbContext;
            _outboxQueue = outboxQueue;
            _domainEvents = domainEvents;
        }

        public Task Execute(Func<Task> action)
        {
            return Execute(action, CancellationToken.None);
        }

        public async Task Execute(Func<Task> action, CancellationToken cancellationToken)
        {
            await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                await action();

                await _outboxQueue.Enqueue(_domainEvents.Events);

                // NOTE: we don't use the built-in notification mechanism,
                // instead we rely on postgres' LISTEN/NOTIFY and a database trigger

                await _dbContext.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }
        }
    }
}