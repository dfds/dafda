using Dafda.Configuration;

namespace Dafda.Consuming
{
    public interface ITopicSubscriberScopeFactory
    {
        TopicSubscriberScope CreateTopicSubscriberScope(IConsumerConfiguration configuration);
    }
}