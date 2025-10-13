using System;
using System.Threading;
using System.Threading.Tasks;
using Dafda.Consuming;

namespace Dafda.Tests.TestDoubles
{
    public class UnitOfWorkStub : IHandlerUnitOfWork
    {
        private readonly object _handlerInstance;

        public UnitOfWorkStub(object handlerInstance)
        {
            _handlerInstance = handlerInstance;
        }

        public async Task Run(Func<object, CancellationToken, Task> handlingAction, CancellationToken cancellationToken)
        {
            await handlingAction(_handlerInstance, cancellationToken);
        }
    }
}