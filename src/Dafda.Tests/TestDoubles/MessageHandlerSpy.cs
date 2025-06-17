using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    public class MessageHandlerSpy<TMessage> : IMessageHandler<TMessage> where TMessage : class, new()
    {
        private readonly Action _onHandle;

        public MessageHandlerSpy(Action onHandle)
        {
            _onHandle = onHandle;
        }

        public Task Handle(TMessage message, MessageHandlerContext context, CancellationToken cancellationToken = default)
        {
            _onHandle?.Invoke();
            return Task.CompletedTask;
        }
    }
}