using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    /// <summary>
    /// Implementations should start a Unit of Work and store the state and
    /// lifecycle in the implementation of <see cref="IOutboxUnitOfWork"/>
    /// </summary>
    public interface IOutboxUnitOfWorkFactory
    {
        /// <summary>
        /// Begin the Unit of Work and create the instance of the custom
        /// <see cref="IOutboxUnitOfWork"/>
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Returns the custom implementation of the <see cref="IOutboxUnitOfWork"/></returns>
        Task<IOutboxUnitOfWork> Begin(CancellationToken cancellationToken);
    }
}