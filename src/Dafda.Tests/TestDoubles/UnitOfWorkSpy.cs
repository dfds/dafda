using System;
using System.Threading.Tasks;
using Dafda.Consuming;
using Dafda.Consuming.Interfaces;

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

        public async Task Run(Func<object, Task> handlingAction)
        {
            _pre?.Invoke();
            await handlingAction(_handlerInstance);
            _post?.Invoke();
        }
    }
}