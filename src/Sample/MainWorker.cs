using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Application;

namespace Sample
{
    public class MainWorker : BackgroundService
    {
        private readonly ILogger<MainWorker> _logger;
        private readonly CommandProcessor _commandProcessor;

        public MainWorker(ILogger<MainWorker> logger, CommandProcessor commandProcessor)
        {
            _logger = logger;
            _commandProcessor = commandProcessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await _commandProcessor.Process(new TestCommand());

                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    public class Test
    {
        public string AggregateId { get; set; }
    }
}