using System;
using System.Threading.Tasks;

namespace Dafda.Consuming
{
    public class ConsumeResult
    {
        private static readonly Action EmptyCommitAction = () => { };
        private readonly Action _onCommit;

        public ConsumeResult(string value, Action onCommit = null)
        {
            Value = value;
            _onCommit = onCommit ?? EmptyCommitAction;
        }

        public string Value { get; }

        public Task Commit()
        {
            _onCommit();
            return Task.CompletedTask;
        }
    }
}