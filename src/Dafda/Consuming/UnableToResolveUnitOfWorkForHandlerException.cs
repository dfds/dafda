using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// Thrown when no <see cref="IHandlerUnitOfWork"/> was returned by
    /// <see cref="IHandlerUnitOfWorkFactory.CreateForHandlerType"/>.
    /// </summary>
    public sealed class UnableToResolveUnitOfWorkForHandlerException : Exception
    {
        public UnableToResolveUnitOfWorkForHandlerException(string message) : base(message)
        {
        }
    }
}