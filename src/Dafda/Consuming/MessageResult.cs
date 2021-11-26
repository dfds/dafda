using System;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    /// <summary>
    /// Resulting Message contaning Transport Level Message
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
        /// Transmitted message
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