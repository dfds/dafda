using System.Collections.Generic;
using Dafda.Consuming;
using Dafda.Messaging;

namespace Dafda.Configuration
{
    public interface IConsumerConfiguration : IConfiguration
    {
        IMessageHandlerRegistry MessageHandlerRegistry { get; }
        IHandlerUnitOfWorkFactory UnitOfWorkFactory { get; }
        
        IInternalConsumerFactory InternalConsumerFactory { get; }
        
        bool EnableAutoCommit { get; }
        IEnumerable<string> SubscribedTopics { get; }
    }
}