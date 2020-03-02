using System.Threading;

namespace Dafda.Outbox
{
    public interface IOutboxListener
    {
        bool Wait(CancellationToken cancellationToken);
    }
}