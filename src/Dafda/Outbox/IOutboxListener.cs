using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    public interface IOutboxListener
    {
        Task<bool> Wait(CancellationToken cancellationToken);
    }
}