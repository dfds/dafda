using System;
using System.Threading;
using System.Threading.Tasks;

namespace Academia.Infrastructure.Persistence
{
    public interface ITransactionalOutbox
    {
        Task Execute(Func<Task> action);
        Task Execute(Func<Task> action, CancellationToken cancellationToken);
    }
}