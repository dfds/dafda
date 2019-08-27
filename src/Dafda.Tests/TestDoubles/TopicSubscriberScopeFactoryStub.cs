using Dafda.Configuration;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    public class TopicSubscriberScopeFactoryStub : ITopicSubscriberScopeFactory
    {
        private readonly TopicSubscriberScope _result;

        public TopicSubscriberScopeFactoryStub(TopicSubscriberScope result)
        {
            _result = result;
        }

        public TopicSubscriberScope CreateTopicSubscriberScope(IConsumerConfiguration configuration)
        {
            return _result;
        }
    }
}