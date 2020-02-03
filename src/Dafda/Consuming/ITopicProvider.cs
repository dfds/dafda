using System.Collections.Generic;

namespace Dafda.Consuming
{
    public interface ITopicProvider
    {
        IEnumerable<string> GetAllSubscribedTopics();
    }
}