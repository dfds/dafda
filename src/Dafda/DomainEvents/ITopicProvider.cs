using System.Collections.Generic;

namespace Dafda.DomainEvents
{
    public interface ITopicProvider
    {
        IEnumerable<string> GetAllSubscribedTopics();
    }
}