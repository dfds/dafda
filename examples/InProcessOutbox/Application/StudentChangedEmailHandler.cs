using System.Threading.Tasks;
using Dafda.Consuming;
using InProcessOutbox.Domain;
using Microsoft.Extensions.Logging;

namespace InProcessOutbox.Application
{
    internal class StudentChangedEmailHandler : IMessageHandler<StudentChangedEmail>
    {
        private readonly ILogger<StudentEnrolledHandler> _logger;

        public StudentChangedEmailHandler(ILogger<StudentEnrolledHandler> logger)
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