using System;
using System.Threading;
using Dafda.Outbox;
using Xunit;

namespace Dafda.Tests.Outbox
{
    public class TestOutboxNotification
    {
        [Fact]
        public void Can_resume_after_timeout_has_expired()
        {
            const int timeout = 50;
            var sut = new OutboxNotification(TimeSpan.FromMilliseconds(timeout));

            var result = sut.Wait(CancellationToken.None);

            Assert.False(result);
        }

        [Fact]
        public void Can_wait_multiple_times()
        {
            const int timeout = 50;
            var sut = new OutboxNotification(TimeSpan.FromMilliseconds(timeout));

            sut.Notify();
            sut.Wait(CancellationToken.None);
            var notified = sut.Wait(CancellationToken.None);

            Assert.False(notified);
        }

        [Fact]
        public void Stop_waiting_when_notified()
        {
            const int timeout = 50;
            var sut = new OutboxNotification(TimeSpan.FromMilliseconds(timeout));

            sut.Notify();
            var result = sut.Wait(CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public void Can_abort_notification()
        {
            const int timeout = 50;

            using (var cts = new CancellationTokenSource(timeout / 2))
            {
                var sut = new OutboxNotification(TimeSpan.FromMilliseconds(-1));

                Assert.Throws<OperationCanceledException>(() => sut.Wait(cts.Token));
            }
        }
    }
}