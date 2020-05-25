using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    /// <summary>
    /// An in-process implementation that handles notification between
    /// the collector and dispatcher in the Dafda outbox. 
    /// </summary>
    public class OutboxNotification : IOutboxListener, IOutboxNotifier, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);
        private readonly TimeSpan _delay;

        private bool _disposed;

        /// <summary>
        /// Initialize an instance of the <see cref="OutboxNotification"/>
        /// </summary>
        /// <param name="delay">The delay between timeouts</param>
        public OutboxNotification(TimeSpan delay)
        {
            _delay = delay;
        }

        /// <inheritdoc />
        public Task Notify(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                return Task.CompletedTask;
            }

            _semaphore.Release();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> Wait(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                return Task.FromResult(false);
            }

            return _semaphore.WaitAsync(_delay, cancellationToken);
        }

        /// <inheritdoc />
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