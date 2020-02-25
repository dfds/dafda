using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    internal class ConsumerScopeFactorySpy : IConsumerScopeFactory
    {
        private readonly ConsumerScope _result;

        public ConsumerScopeFactorySpy(ConsumerScope result)
        {
            _result = result;
        }

        public ConsumerScope CreateConsumerScope()
        {
            CreateConsumerScopeCalled++;
            return _result;
        }

        public int CreateConsumerScopeCalled { get; private set; }
    }
}