using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class SubscriberHostedService : BackgroundService
    {
        private readonly ILogger<SubscriberHostedService> _logger;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly TopicSubscriber _topicSubscriber;

        public SubscriberHostedService(ILogger<SubscriberHostedService> logger, IApplicationLifetime applicationLifetime, TopicSubscriber topicSubscriber)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _topicSubscriber = topicSubscriber;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                try
                {
                    _logger.LogDebug("SubscriberHostedService started");
                    //await _topicSubscriber.Start(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("SubscriberHostedService cancelled");
                }
                catch (Exception err)
                {
                    _logger.LogError(err, "Unhandled error occurred while consuming messaging");
                    _applicationLifetime.StopApplication();
                }
            }, stoppingToken);
        }
    }
}