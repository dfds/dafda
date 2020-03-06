using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.Infrastructure.Persistence
{
    public class Transactional
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Transactional(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task Execute<T>(Func<T, Task> action)
        {
            return Execute(action, CancellationToken.None);
        }

        public async Task Execute<T>(Func<T, Task> action, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<T>();
            var outboxQueue = scope.ServiceProvider.GetRequiredService<OutboxQueue>();
            var domainEvents = scope.ServiceProvider.GetRequiredService<DomainEvents>();

            IOutboxNotifier outboxNotifier;

            await using (var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                await action(service);

                outboxNotifier = await outboxQueue.Enqueue(domainEvents.Events);

                await dbContext.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }

            if (outboxNotifier != null)
            {
                await outboxNotifier.Notify(cancellationToken); // NOTE: when using postgres LISTEN/NOTIFY this should/could be part of the transaction scope above
            }
        }
    }
}