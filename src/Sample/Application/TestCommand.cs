using System.Threading.Tasks;
using Dafda.Outbox;
using Microsoft.Extensions.Logging;

namespace Sample.Application
{
    public class TestCommand : ICommand
    {
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        private readonly ILogger<TestCommandHandler> _logger;
        private readonly OutboxQueue _outboxQueue;

        public TestCommandHandler(ILogger<TestCommandHandler> logger, OutboxQueue outboxQueue)
        {
            _logger = logger;
            _outboxQueue = outboxQueue;
        }

        public Task Handle(TestCommand command)
        {
            _logger.LogInformation("TEST");

            return _outboxQueue.Enqueue(new[] {new Test {AggregateId = "aggregate-id"}});
        }
    }
}