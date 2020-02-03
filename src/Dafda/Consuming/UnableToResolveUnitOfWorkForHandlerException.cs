using System;

namespace Dafda.Consuming
{
    public class UnableToResolveUnitOfWorkForHandlerException : Exception
    {
        public UnableToResolveUnitOfWorkForHandlerException(string message) : base(message)
        {
            
        }
    }
}