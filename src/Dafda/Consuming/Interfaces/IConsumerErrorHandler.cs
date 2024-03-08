using Dafda.Configuration;
using System;
using System.Threading.Tasks;

namespace Dafda.Consuming.Interfaces
{
    /// <summary>Interface for error handler while consuming</summary>
    public interface IConsumerErrorHandler
    {
        /// <summary>Main handle method</summary>
        Task<ConsumerFailureStrategy> Handle(Exception exception);
    }
}
