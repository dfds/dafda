using System;
using System.Threading;

namespace Dafda.Outbox
{
    internal class OutboxWaiter : IOutboxWaiter, IDisposable
    {
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private readonly TimeSpan _delay;

        public OutboxWaiter(TimeSpan delay)
        {
            _delay = delay;
        }
        
        public void WakeUp()
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