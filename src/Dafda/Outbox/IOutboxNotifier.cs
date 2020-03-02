using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    public interface IOutboxNotifier
    {
        Task Notify(CancellationToken cancellationToken);
    }
}