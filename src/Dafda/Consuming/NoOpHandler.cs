using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    /// <summary>
    /// A message handler that will just log the message type and id without performing any further processing.
    /// </summary>
    public sealed class NoOpHandler: IMessageHandler<object>
    {
        /// <summary>
        /// The injected logger for this handler
        /// </summary>
        public ILogger<NoOpHandler> Logger { get; }

        /// <summary>
        /// Instantiates the class with the provided logger
        /// </summary>
        public NoOpHandler(ILogger<NoOpHandler> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Logs a debug message with the Id and Type from the provided <paramref name="context"/>, and returns a completed task
        /// </summary>
        public Task Handle(object _, MessageHandlerContext context, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation(
                "Dafda is ignoring a message of type {messageType} with id {messageId}",
                context.MessageType,
                context.MessageId);
            return Task.CompletedTask;
        }
    }
}
