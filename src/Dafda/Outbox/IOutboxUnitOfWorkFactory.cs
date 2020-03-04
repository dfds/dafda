using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    public interface IOutboxUnitOfWorkFactory
    {
        Task<IOutboxUnitOfWork> Begin(CancellationToken cancellationToken);
    }
}