using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    public class OutboxNotification : IOutboxListener, IOutboxNotifier, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);
        private readonly TimeSpan _delay;

        private bool _disposed;

        public OutboxNotification(TimeSpan delay)
        {
            _delay = delay;
        }

        public Task Notify(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                return Task.CompletedTask;
            }

            _semaphore.Release();
            return Task.CompletedTask;
        }

        public Task<bool> Wait(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                return Task.FromResult<bool>(false);
            }

            return _semaphore.WaitAsync(_delay, cancellationToken);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _semaphore.Dispose();
            _disposed = true;
        }
    }

}