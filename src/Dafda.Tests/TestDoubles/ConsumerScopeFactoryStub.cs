using Dafda.Consuming;
using Dafda.Consuming.Interfaces;

namespace Dafda.Tests.TestDoubles
{
    internal class ConsumerScopeFactoryStub : IConsumerScopeFactory<MessageResult>
    {
        private readonly IConsumerScope<MessageResult> _result;

        public ConsumerScopeFactoryStub(IConsumerScope<MessageResult> result)
        {
            _result = result;
        }

        public IConsumerScope<MessageResult> CreateConsumerScope()
        {
            return _result;
        }
    }
}