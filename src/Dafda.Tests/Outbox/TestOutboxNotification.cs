using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Outbox;
using Xunit;

namespace Dafda.Tests.Outbox
{
    public class TestOutboxNotification
    {
        [Fact]
        public async Task Can_resume_after_timeout_has_expired()
        {
            const int timeout = 50;
            var sut = new OutboxNotification(TimeSpan.FromMilliseconds(timeout));

            var result = await sut.Wait(CancellationToken.None);

            Assert.False(result);
        }

        [Fact]
        public async Task Stop_waiting_when_notified()
        {
            const int timeout = 50;
            var sut = new OutboxNotification(TimeSpan.FromMilliseconds(timeout));

            var notifyTask = Task.Delay(timeout / 2).ContinueWith(_ =>
            {
                sut.Notify(CancellationToken.None);
                return true;
            });
            var waitTask = sut.Wait(CancellationToken.None);

            var results = await Task.WhenAll(notifyTask, waitTask);

            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task Can_abort_notification()
        {
            const int timeout = 50;
            var sut = new OutboxNotification(TimeSpan.FromMilliseconds(timeout));

            using (var cts = new CancellationTokenSource(timeout / 2))
            {
                await Assert.ThrowsAsync<OperationCanceledException>(() => sut.Wait(cts.Token));
            }
        }
    }
}