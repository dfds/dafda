using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Consuming.Interfaces;

namespace Dafda.Tests.TestDoubles
{
    internal class ConsumerScopeStub : IConsumerScope<MessageResult>
    {
        private readonly MessageResult _result;

        public ConsumerScopeStub(MessageResult result)
        {
            _result = result;
        }

        public Task<MessageResult> GetNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }

        public void Dispose()
        {

        }
    }
}