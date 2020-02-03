using System.Threading.Tasks;
using Dafda.Consuming;
using Microsoft.Extensions.Logging;

namespace Sample.Application
{
    internal class TestHandler : IMessageHandler<Test>
    {
        private readonly ILogger<TestHandler> _logger;

        public TestHandler(ILogger<TestHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(Test message)
        {
            _logger.LogInformation($@"{this.GetType().Name} handled: {{@Message}}", message);

            return Task.CompletedTask;
        }
    }

    internal class AnotherTestHandler : IMessageHandler<Test>
    {
        private readonly ILogger<TestHandler> _logger;

        public AnotherTestHandler(ILogger<TestHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(Test message)
        {
            _logger.LogInformation($@"{this.GetType().Name} handled: {{@Message}}", message);
           
            return Task.CompletedTask;
        }
    }
}