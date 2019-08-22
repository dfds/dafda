using System;
using System.Threading;

namespace Dafda.Consuming
{
    public interface IInternalConsumer : IDisposable
    {
        ConsumeResult Consume(CancellationToken cancellationToken);
    }
}