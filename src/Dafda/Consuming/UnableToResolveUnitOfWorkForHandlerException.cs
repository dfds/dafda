using System;

namespace Dafda.Consuming
{
    public sealed class UnableToResolveUnitOfWorkForHandlerException : Exception
    {
        internal UnableToResolveUnitOfWorkForHandlerException(string message) : base(message)
        {
        }
    }
}