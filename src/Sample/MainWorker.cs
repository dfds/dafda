using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Infrastructure.Persistence;

namespace Sample
{
    public class MainWorker : BackgroundService
    {
        private readonly ILogger<MainWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Stats _stats;

        public MainWorker(ILogger<MainWorker> logger, IServiceScopeFactory serviceScopeFactory, Stats stats)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _stats = stats;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);

                    IOutboxNotifier outboxNotifier;
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            var outboxQueue = scope.ServiceProvider.GetRequiredService<OutboxQueue>();

                            outboxNotifier = await outboxQueue.Enqueue(new[] {new TestEvent {AggregateId = "aggregate-id"}});

                            await dbContext.SaveChangesAsync(stoppingToken);
                            transaction.Commit();
                        }
                    }

                    if (outboxNotifier != null)
                    {
                        await outboxNotifier.Notify(stoppingToken); // NOTE: when using postgres LISTEN/NOTIFY this should/could be part of the transaction scope above
                    }

                    _logger.LogInformation("{Stats}", _stats.ToString());

                    await Task.Delay(1000, stoppingToken);
                }
            }, stoppingToken);
        }
    }

    public class TestEvent
    {
        public string AggregateId { get; set; }
    }
}