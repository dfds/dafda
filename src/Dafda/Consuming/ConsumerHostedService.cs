using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class ConsumerHostedService : BackgroundService
    {
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly Consumer _consumer;
        private readonly string _groupId;
        private readonly ConsumerErrorHandler _consumerErrorHandler;

        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, IHostApplicationLifetime applicationLifetime, Consumer consumer, string groupId, ConsumerErrorHandler consumerErrorHandler)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _consumer = consumer;
            _groupId = groupId;
            _consumerErrorHandler = consumerErrorHandler;
        }

        public async Task ConsumeAll(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    _logger.LogDebug("ConsumerHostedService [{GroupId}] started", _groupId);
                    await _consumer.ConsumeAll(stoppingToken);
                    break;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("ConsumerHostedService [{GroupId}] cancelled", _groupId);
                    break;
                }
                catch (Exception err)
                {
                    _logger.LogError(err, "Unhandled error occurred while consuming messaging");
                    var failureStrategy = await _consumerErrorHandler.Handle(err);
                    if (failureStrategy == ConsumerFailureStrategy.Default)
                    {
                        _applicationLifetime.StopApplication();
                        break;
                    }
                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () => { await ConsumeAll(stoppingToken); }, stoppingToken);
        }
    }
}