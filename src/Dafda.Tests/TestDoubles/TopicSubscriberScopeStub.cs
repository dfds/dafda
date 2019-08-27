using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    public class TopicSubscriberScopeStub : TopicSubscriberScope
    {
        private readonly MessageResult _result;

        public TopicSubscriberScopeStub(MessageResult result)
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