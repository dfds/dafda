using System.Collections.Generic;

namespace Dafda.Messaging
{
    public interface ITopicProvider
    {
        IEnumerable<string> GetAllSubscribedTopics();
    }
}