using System;
using System.Threading;

namespace Dafda.Consuming
{
    public interface IConsumer : IDisposable
    {
        ConsumeResult Consume(CancellationToken cancellationToken);
    }
}