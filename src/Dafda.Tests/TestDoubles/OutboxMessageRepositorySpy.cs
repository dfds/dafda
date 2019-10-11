using System.Collections.Generic;
using System.Threading.Tasks;
using Dafda.Outbox;

namespace Dafda.Tests.TestDoubles
{
    public class OutboxMessageRepositorySpy : IOutboxMessageRepository
    {
        public List<OutboxMessage> OutboxMessages { get; } = new List<OutboxMessage>();

        public Task Add(IEnumerable<OutboxMessage> outboxMessages)
        {
            OutboxMessages.AddRange(outboxMessages);
            return Task.CompletedTask;
        }
    }
}