using System;

namespace Dafda.Consuming
{
    public interface IHandlerUnitOfWorkFactory
    {
        IHandlerUnitOfWork CreateForHandlerType(Type handlerType);
    }
}