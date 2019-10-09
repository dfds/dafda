using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dafda.Outbox
{
    public interface IOutboxMessageRepository
    {
        Task Add(IEnumerable<OutboxMessage> outboxMessages);
    }
}