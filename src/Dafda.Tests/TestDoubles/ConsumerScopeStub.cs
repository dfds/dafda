using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    internal class ConsumerScopeStub : ConsumerScope
    {
        private readonly MessageResult _result;

        public ConsumerScopeStub(MessageResult result)
        {
            _result = result;
        }

        public override Task<MessageResult> GetNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }

        public override void Dispose()
        {
            
        }
    }
}