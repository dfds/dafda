using System.Collections.Generic;
using Dafda.Configuration;

namespace Dafda.Consuming
{
    public interface IConsumerFactory
    {
        IConsumer CreateConsumer(IConfiguration configuration, IEnumerable<string> topics);
    }
}