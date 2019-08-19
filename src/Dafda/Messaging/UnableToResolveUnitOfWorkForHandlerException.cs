using System;

namespace Dafda.Messaging
{
    public class UnableToResolveUnitOfWorkForHandlerException : Exception
    {
        public UnableToResolveUnitOfWorkForHandlerException(string message) : base(message)
        {
            
        }
    }
}