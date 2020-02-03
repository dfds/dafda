using System.Collections.Generic;
using Dafda.Consuming;

namespace Dafda.Configuration
{
    public interface IConsumerConfiguration : IConfiguration
    {
        IMessageHandlerRegistry MessageHandlerRegistry { get; }
        IHandlerUnitOfWorkFactory UnitOfWorkFactory { get; }
        
        IConsumerScopeFactory ConsumerScopeFactory { get; }
        
        bool EnableAutoCommit { get; }
        IEnumerable<string> SubscribedTopics { get; }
        string GroupId { get; }
    }
}