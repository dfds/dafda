using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Consuming;

namespace Dafda.Messaging
{
    public class Consumer
    {
        private readonly IConsumerConfiguration _configuration;
        private readonly ITopicSubscriberScopeFactory _topicSubscriberScopeFactory;
        private readonly LocalMessageDispatcher _localMessageDispatcher;

        public Consumer(IConsumerConfiguration configuration)
        {
            _configuration = configuration;
            _localMessageDispatcher = new LocalMessageDispatcher(configuration.MessageHandlerRegistry, configuration.UnitOfWorkFactory);
            _topicSubscriberScopeFactory = _configuration.TopicSubscriberScopeFactory;
        }

        public async Task ConsumeAll(CancellationToken cancellationToken)
        {
            using (var subscriberScope = _topicSubscriberScopeFactory.CreateTopicSubscriberScope(_configuration))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await ProcessNextMessage(subscriberScope, cancellationToken);
                }
            }
        }

        public async Task ConsumeSingle(CancellationToken cancellationToken)
        {
            using (var subscriberScope = _topicSubscriberScopeFactory.CreateTopicSubscriberScope(_configuration))
            {
                await ProcessNextMessage(subscriberScope, cancellationToken);
            }
        }

        private async Task ProcessNextMessage(TopicSubscriberScope topicSubscriberScope, CancellationToken cancellationToken)
        {
            var messageResult = await topicSubscriberScope.GetNext(cancellationToken);
            await _localMessageDispatcher.Dispatch(messageResult.Message);

            await messageResult.Commit();
        }
    }
}