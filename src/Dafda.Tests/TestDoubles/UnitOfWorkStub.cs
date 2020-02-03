using System;
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

        public async Task Run(Func<object, Task> handlingAction)
        {
            await handlingAction(_handlerInstance);
        }
    }
}