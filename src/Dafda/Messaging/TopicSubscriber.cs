using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;

namespace Dafda.Messaging
{
    public class TopicSubscriber
    {
        private readonly IConsumerFactory _consumerFactory;
        private readonly ILocalMessageDispatcher _localMessageDispatcher;

        public TopicSubscriber(IConsumerFactory consumerFactory, ILocalMessageDispatcher localMessageDispatcher)
        {
            _consumerFactory = consumerFactory;
            _localMessageDispatcher = localMessageDispatcher;
        }

        public async Task Start(IConfiguration configuration, IEnumerable<string> topicNames, CancellationToken cancellationToken)
        {
            using (var consumer = _consumerFactory.CreateConsumer(configuration, topicNames))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ProcessNextMessage(consumer, cancellationToken);
                }
            }
        }

        public async Task ProcessNextMessage(IConsumer consumer, CancellationToken cancellationToken)
        {
            var consumeResult = consumer.Consume(cancellationToken);
            var message = new JsonMessageEmbeddedDocument(consumeResult.Value);

            await _localMessageDispatcher.Dispatch(message);

            await consumeResult.Commit();
        }
    }
}