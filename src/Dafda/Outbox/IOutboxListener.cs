using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;
using Dafda.Configuration.Outbox;

namespace Dafda.Outbox
{
    /// <summary>
    /// Implement and override using the <see cref="OutboxProducerOptions.WithListener"/> to allow
    /// for signalling between the outbox collector and dispatcher. 
    /// </summary>
    public interface IOutboxListener
    {
        /// <summary>
        /// Wait for outbox notifications. 
        /// </summary>
        /// <param name="cancellationToken">The token to abort the waiting; typically the
        /// application shutdown token.</param>
        /// <returns>Optionally returns true; to indicate that the listener was triggered
        /// before a timeout; otherwise false</returns>
        Task<bool> Wait(CancellationToken cancellationToken);
    }
}