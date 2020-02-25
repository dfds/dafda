using System;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    internal sealed class MessageResult
    {
        private static readonly Func<Task> EmptyCommitAction = () => Task.CompletedTask;
        private readonly Func<Task> _onCommit;

        public MessageResult(TransportLevelMessage message, Func<Task> onCommit = null)
        {
            Message = message;
            _onCommit = onCommit ?? EmptyCommitAction;
        }

        public TransportLevelMessage Message { get; }

        public async Task Commit()
        {
            await _onCommit();
        }
    }
}