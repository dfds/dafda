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

        public MainWorker(ILogger<MainWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);


                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
                        using (var transaction = dbContext.Database.BeginTransaction())
                        {
                            var outboxQueue = scope.ServiceProvider.GetRequiredService<OutboxQueue>();

                            await outboxQueue.Enqueue(new[] {new TestEvent {AggregateId = "aggregate-id"}});

                            await dbContext.SaveChangesAsync(stoppingToken);
                            transaction.Commit();
                        }

                        var waiter = scope.ServiceProvider.GetRequiredService<IOutboxWaiter>();
                        waiter.WakeUp();
                    }

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