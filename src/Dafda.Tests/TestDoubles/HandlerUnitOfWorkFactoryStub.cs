using System;
using Dafda.Messaging;

namespace Dafda.Tests.TestDoubles
{
    public class HandlerUnitOfWorkFactoryStub : IHandlerUnitOfWorkFactory
    {
        private readonly IHandlerUnitOfWork _result;

        public HandlerUnitOfWorkFactoryStub(IHandlerUnitOfWork result)
        {
            _result = result;
        }

        public IHandlerUnitOfWork CreateForHandlerType(Type handlerType)
        {
            return _result;
        }
    }
}