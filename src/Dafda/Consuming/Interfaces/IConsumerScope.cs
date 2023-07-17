using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Dafda.Consuming.Interfaces
{
    /// <summary>
    /// Interface version of ConsumerScope
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IConsumerScope<TResult> : IDisposable
    {
        /// <summary>
        /// Getting next result
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        abstract Task<TResult> GetNext(CancellationToken cancellationToken);
    }
}
