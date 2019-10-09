using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    public interface IOutbox
    {
        Task Enqueue(IEnumerable<object> events);
    }
}