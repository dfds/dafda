using System;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    /// <summary>
    /// Object that contains message when consumed from Kafka.
    /// To be used for message handling prior to dispatching to the handlers.
    /// </summary>
    public class MessageResult
    {
        private static readonly Func<Task> EmptyCommitAction = () => Task.CompletedTask;
        private readonly Func<Task> _onCommit;

        /// <summary>
        /// Resulting Message contaning Transport Level Message
        /// </summary>
        public MessageResult(TransportLevelMessage message, Func<Task> onCommit = null)
        {
            Message = message;
            _onCommit = onCommit ?? EmptyCommitAction;
        }

        /// <summary>
        /// Transmitted message consumed from Kafka
        /// </summary>
        public TransportLevelMessage Message { get; }

        /// <summary>
        /// Commit message to handlers
        /// </summary>
        public async Task Commit()
        {
            await _onCommit();
        }
    }
}