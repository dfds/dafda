using System;

namespace Dafda.Consuming
{
    public sealed class UnableToResolveUnitOfWorkForHandlerException : Exception
    {
        public UnableToResolveUnitOfWorkForHandlerException(string message) : base(message)
        {
            
        }
    }
}