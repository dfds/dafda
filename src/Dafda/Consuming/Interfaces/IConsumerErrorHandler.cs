using Dafda.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dafda.Consuming.Interfaces
{
    public interface IConsumerErrorHandler
    {
        Task<ConsumerFailureStrategy> Handle(Exception exception);
    }
}
