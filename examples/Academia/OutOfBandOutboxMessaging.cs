using System;
using System.Threading;
using System.Threading.Tasks;
using Academia.Application;
using Academia.Domain;
using Academia.Infrastructure.Persistence;
using Dafda.Configuration;
using Dafda.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Academia
{
    public static class OutOfBandOutboxMessaging
    {
        public static void AddOutOfBandOutboxMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ITransactionalOutbox, TransactionalOutbox>();

            // configure messaging: consumer
            services.AddConsumer(options =>
            {
                // kafka consumer settings
                options.WithConfigurationSource(configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA", "ACADEMIA_KAFKA");

                // register message handlers
                options.RegisterMessageHandler<StudentEnrolled, StudentEnrolledHandler>("academia.students", "student-enrolled");
                options.RegisterMessageHandler<StudentChangedEmail, StudentChangedEmailHandler>("academia.students", "student-changed-email");
            });

            // configure the outbox pattern using Dafda
            services.AddOutbox(options =>
            {
                // register outgoing (through the outbox) messages
                options.Register<StudentEnrolled>("academia.students", "student-enrolled", @event => @event.StudentId);
                options.Register<StudentChangedEmail>("academia.students", "student-changed-email", @event => @event.StudentId);

                // include outbox persistence
                options.WithOutboxEntryRepository<OutboxEntryRepository>();

                // no notifier configured
            });

            // the outbox producer is configured out-of-band
        }

        private class TransactionalOutbox : ITransactionalOutbox
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
}