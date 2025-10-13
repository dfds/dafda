using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    public class UnitOfWorkSpy : IHandlerUnitOfWork
    {
        private readonly object _handlerInstance;
        private readonly Action _pre;
        private readonly Action _post;

        public UnitOfWorkSpy(object handlerInstance, Action pre = null, Action post = null)
        {
            _handlerInstance = handlerInstance;
            _pre = pre;
            _post = post;
        }

        public async Task Run(Func<object, CancellationToken, Task> handlingAction, CancellationToken cancellationToken)
        {
            _pre?.Invoke();
            await handlingAction(_handlerInstance, cancellationToken);
            _post?.Invoke();
        }
    }
}