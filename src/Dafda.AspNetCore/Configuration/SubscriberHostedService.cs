using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dafda.Configuration
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
                    await _topicSubscriber.Start(stoppingToken);
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

    internal class TopicSubscriber
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<TopicSubscriber> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly MessageHandlerRegistry _messageHandlerRegistry;
        private readonly IConfiguration _configuration;

        public TopicSubscriber(ILoggerFactory loggerFactory, ILogger<TopicSubscriber> logger, IServiceProvider serviceProvider, MessageHandlerRegistry messageHandlerRegistry, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _messageHandlerRegistry = messageHandlerRegistry;
            _configuration = configuration;
        }

        public async Task Start(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Starting subscriber....");

            using (var consumer = new ConsumerBuilder<string, string>(_configuration).Build())
            {
                foreach (var subscribedTopic in _messageHandlerRegistry.GetAllSubscribedTopics())
                {
                    _logger.LogDebug("Consuming from {Topic}", subscribedTopic);
                    consumer.Subscribe(subscribedTopic);
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var messageProcessor = new ServiceProviderBasedMessageProcessingScope(_loggerFactory, consumer, _serviceProvider))
                    {
                        await messageProcessor.Process(stoppingToken);
                    }
                }
            }
        }
    }
}