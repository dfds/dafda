using System;
using System.Threading;

namespace Dafda.Outbox
{
    public class OutboxNotification : IOutboxListener, IOutboxNotifier, IDisposable
    {
        private readonly ManualResetEventSlim _resetEvent = new ManualResetEventSlim(false);

        private readonly TimeSpan _delay;

        private bool _disposed;

        public OutboxNotification(TimeSpan delay)
        {
            _delay = delay;
        }

        public void Notify()
        {
            if (_disposed)
            {
                return;
            }

            _resetEvent.Set();
        }

        public bool Wait(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                return false;
            }

            var wasSignaled = _resetEvent.Wait(_delay, cancellationToken);

            _resetEvent.Reset();

            return wasSignaled;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _resetEvent.Dispose();
            _disposed = true;
        }
    }
}