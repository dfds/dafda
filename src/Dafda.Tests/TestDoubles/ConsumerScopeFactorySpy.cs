using Dafda.Consuming;
using Dafda.Consuming.Interfaces;

namespace Dafda.Tests.TestDoubles
{
    internal class ConsumerScopeFactorySpy : IConsumerScopeFactory<MessageResult>
    {
        private readonly IConsumerScope<MessageResult> _result;

        public ConsumerScopeFactorySpy(IConsumerScope<MessageResult> result)
        {
            _result = result;
        }

        public IConsumerScope<MessageResult> CreateConsumerScope()
        {
            CreateConsumerScopeCalled++;
            return _result;
        }

        public int CreateConsumerScopeCalled { get; private set; }
    }
}