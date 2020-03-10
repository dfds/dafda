using System.Threading.Tasks;
using Academia.Domain;
using Dafda.Consuming;
using Microsoft.Extensions.Logging;

namespace Academia.Application
{
    public class StudentChangedEmailHandler : IMessageHandler<StudentChangedEmail>
    {
        private readonly ILogger<StudentChangedEmailHandler> _logger;

        public StudentChangedEmailHandler(ILogger<StudentChangedEmailHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(StudentChangedEmail message, MessageHandlerContext context)
        {
            _logger.LogDebug($@"{this.GetType().Name} handled: {{@Message}}", message);

            return Task.CompletedTask;
        }
    }
}