using System.Threading.Tasks;
using Dafda.Messaging;
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
            _logger.LogInformation(@"Handled: {@Message}", message);
            
            return Task.CompletedTask;
        }
    }
}