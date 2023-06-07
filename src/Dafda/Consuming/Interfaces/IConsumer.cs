using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Consuming.Interfaces
{
    internal interface IConsumer
    {
        Task ConsumeAll(CancellationToken cancellationToken);
        Task ConsumeSingle(CancellationToken cancellationToken);
    }
}