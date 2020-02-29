using System;
using System.Threading;

namespace Dafda.Outbox
{
    public class OutboxNotification : IOutboxNotification, IDisposable
    {
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
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

            _autoResetEvent.Set();
        }

        public void Wait()
        {
            if (_disposed)
            {
                return;
            }

            _autoResetEvent.WaitOne(_delay);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _autoResetEvent.Dispose();
            _disposed = true;
        }
    }
}