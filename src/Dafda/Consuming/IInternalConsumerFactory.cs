using System.Collections.Generic;
using Dafda.Configuration;

namespace Dafda.Consuming
{
    public interface IInternalConsumerFactory
    {
        IInternalConsumer CreateConsumer(IConsumerConfiguration configuration);
    }
}