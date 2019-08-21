using System;

namespace Dafda.Messaging
{
    public interface IHandlerUnitOfWorkFactory
    {
        IHandlerUnitOfWork CreateForHandlerType(Type handlerType);
    }
}