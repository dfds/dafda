using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dafda.Configuration;
using Dafda.Messaging;
using Microsoft.Extensions.Logging;

namespace Dafda.Consuming
{
    internal class TopicSubscriber
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<TopicSubscriber> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITopicProvider _topicProvider;
        private readonly IConfiguration _configuration;

        public TopicSubscriber(ILoggerFactory loggerFactory, ILogger<TopicSubscriber> logger, IServiceProvider serviceProvider, ITopicProvider topicProvider, IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _topicProvider = topicProvider;
            _configuration = configuration;
        }

        public async Task Start(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Starting subscriber....");

            using (var consumer = new ConsumerBuilder<string, string>(_configuration).Build())
            {
                foreach (var subscribedTopic in _topicProvider.GetAllSubscribedTopics())
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