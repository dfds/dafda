using System;
using System.Threading;

namespace Dafda.Outbox
{
    public class OutboxNotification : IOutboxNotification, IDisposable
    {
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly TimeSpan _delay;

        public OutboxNotification(TimeSpan delay)
        {
            _delay = delay;
        }
        
        public void Notify()
        {
            _autoResetEvent.Set();
        }

        public void Wait()
        {
            _autoResetEvent.WaitOne(_delay);
        }

        public void Dispose()
        {
            _autoResetEvent.Dispose();
        }
    }
}