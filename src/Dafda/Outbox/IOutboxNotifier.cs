using System.Threading;
using System.Threading.Tasks;
using Dafda.Configuration;

namespace Dafda.Outbox
{
    /// <summary>
    /// Implement and override using the <see cref="OutboxOptions.WithNotifier"/> to allow
    /// for signalling between the outbox collector and dispatcher. 
    /// </summary>
    public interface IOutboxNotifier
    {
        /// <summary>
        /// Notify the Dafda outbox dispatcher that a new message are available.  
        /// </summary>
        /// <param name="cancellationToken">The token to abort the waiting; typically the
        /// application shutdown token.</param>
        Task Notify(CancellationToken cancellationToken);
    }
}