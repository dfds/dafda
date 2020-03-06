using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Infrastructure.Persistence;

namespace Sample
{
    public class MainWorker : BackgroundService
    {
        private readonly ILogger<MainWorker> _logger;
        private readonly Transactional _transactional;
        private readonly Stats _stats;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public MainWorker(ILogger<MainWorker> logger, Transactional transactional, Stats stats, IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _transactional = transactional;
            _stats = stats;
            _applicationLifetime = applicationLifetime;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
                    {
                        while (!stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);

                            await _transactional.Execute<ApplicationService>(service => service.Process(), stoppingToken);

                            _logger.LogInformation("{Stats}", _stats.ToString());

                            await Task.Delay(1000, stoppingToken);
                        }
                    }, stoppingToken)
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            var exception = task.Exception?.InnerException;

                            _logger.LogError(exception, "Background thread failed");
                            _applicationLifetime.StopApplication();
                        }
                    }, stoppingToken)
                ;
        }
    }
}