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
    public static class InProcessOutboxMessaging
    {
        public static void AddInProcessOutboxMessaging(this IServiceCollection services, IConfiguration configuration)
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

            // register the outbox in-process notification mechanism
            var outboxNotification = new OutboxNotification(TimeSpan.FromSeconds(5));
            services.AddSingleton(provider => outboxNotification); // register to dispose

            // configure the outbox pattern using Dafda
            services.AddOutbox(options =>
            {
                // register outgoing (through the outbox) messages
                options.Register<StudentEnrolled>("academia.students", "student-enrolled", @event => @event.StudentId);
                options.Register<StudentChangedEmail>("academia.students", "student-changed-email", @event => @event.StudentId);

                // include outbox persistence
                options.WithOutboxEntryRepository<OutboxEntryRepository>();

                // add notifier (for immediate dispatch)
                options.WithNotifier(outboxNotification);
            });

            // configure the outbox producer
            services.AddOutboxProducer(options =>
            {
                // kafka producer settings
                options.WithConfigurationSource(configuration);
                options.WithEnvironmentStyle("DEFAULT_KAFKA", "ACADEMIA_KAFKA");

                // include outbox unit of work (so we can read/update the outbox table)
                options.WithUnitOfWorkFactory<OutboxUnitOfWorkFactory>();

                // add listener (for immediate dispatch)
                options.WithListener(outboxNotification);
            });
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
}