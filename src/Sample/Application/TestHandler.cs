using System.Threading.Tasks;
using Dafda.Consuming;
using Microsoft.Extensions.Logging;

namespace Sample.Application
{
    internal class TestHandler : IMessageHandler<TestEvent>
    {
        private readonly ILogger<TestHandler> _logger;
        private readonly Stats _stats;

        public TestHandler(ILogger<TestHandler> logger, Stats stats)
        {
            _logger = logger;
            _stats = stats;
        }

        public Task Handle(TestEvent message, MessageHandlerContext context)
        {
            _logger.LogDebug($@"{this.GetType().Name} handled: {{@Message}}", message);

            _stats.Consume();
            
            _logger.LogInformation("{Stats}", _stats.ToString());

            return Task.CompletedTask;
        }
    }

    internal class AnotherTestHandler : IMessageHandler<TestEvent>
    {
        private readonly ILogger<TestHandler> _logger;

        public AnotherTestHandler(ILogger<TestHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(TestEvent message, MessageHandlerContext context)
        {
            _logger.LogDebug($@"{this.GetType().Name} handled: {{@Message}}", message);

            return Task.CompletedTask;
        }
    }
}