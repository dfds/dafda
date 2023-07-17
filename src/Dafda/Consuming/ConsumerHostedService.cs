using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    /// <summary>
    /// Hosted service for the consumer
    /// </summary>
    public class ConsumerHostedService : BackgroundService
    {
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IConsumer _consumer;
        private readonly string _groupId;
        private readonly IConsumerErrorHandler _consumerErrorHandler;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="applicationLifetime"></param>
        /// <param name="consumer"></param>
        /// <param name="groupId"></param>
        /// <param name="consumerErrorHandler"></param>
        public ConsumerHostedService(ILogger<ConsumerHostedService> logger, IHostApplicationLifetime applicationLifetime, IConsumer consumer, string groupId, IConsumerErrorHandler consumerErrorHandler)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _consumer = consumer;
            _groupId = groupId;
            _consumerErrorHandler = consumerErrorHandler;
        }

        /// <summary>
        /// Consume all messges
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Override of ExecuteAsync
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () => { await ConsumeAll(stoppingToken); }, stoppingToken);
        }
    }
}