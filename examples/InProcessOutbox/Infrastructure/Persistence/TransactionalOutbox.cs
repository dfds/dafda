using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using InProcessOutbox.Domain;

namespace InProcessOutbox.Infrastructure.Persistence
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
            IOutboxNotifier outboxNotifier;

            await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                await action();

                outboxNotifier = await _outboxQueue.Enqueue(_domainEvents.Events);

                await _dbContext.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }

            if (outboxNotifier != null)
            {
                // NOTE: when using postgres LISTEN/NOTIFY this should/could be part of the transaction scope above
                await outboxNotifier.Notify(cancellationToken);
            }
        }
    }
}