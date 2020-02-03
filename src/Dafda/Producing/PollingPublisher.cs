using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.Extensions.Hosting;

namespace Dafda.Producing
{
    internal class PollingPublisher : BackgroundService
    {
        private readonly OutboxProcessor _outboxProcessor;

        public PollingPublisher(IOutboxUnitOfWorkFactory unitOfWorkFactory, IProducer producer)
        {
            _outboxProcessor = new OutboxProcessor(unitOfWorkFactory, producer);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessUnpublishedOutboxMessages(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }, stoppingToken);
        }

        public Task ProcessUnpublishedOutboxMessages(CancellationToken stoppingToken)
        {
            return _outboxProcessor.ProcessUnpublishedOutboxMessages(stoppingToken);
        }
    }
}