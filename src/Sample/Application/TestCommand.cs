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
        private readonly IOutbox _outbox;

        public TestCommandHandler(ILogger<TestCommandHandler> logger, IOutbox outbox)
        {
            _logger = logger;
            _outbox = outbox;
        }

        public Task Handle(TestCommand command)
        {
            _logger.LogInformation("TEST");

            return _outbox.Enqueue(new[] {new Test {AggregateId = "aggregate-id"}});
        }
    }
}