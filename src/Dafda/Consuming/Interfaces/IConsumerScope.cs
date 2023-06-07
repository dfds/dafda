using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Consuming.Interfaces
{
    internal interface IConsumerScope<TResult> : IDisposable
    {
        abstract Task<TResult> GetNext(CancellationToken cancellationToken);
    }
}