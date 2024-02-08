using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// Thrown when no <see cref="IHandlerUnitOfWork"/> was returned by
    /// <see cref="IHandlerUnitOfWorkFactory.CreateForHandlerType"/>.
    /// </summary>
    public sealed class UnableToResolveUnitOfWorkForHandlerException : Exception
    {
        /// <summary>Exceptions constructor</summary>
        public UnableToResolveUnitOfWorkForHandlerException(string message) : base(message)
        {
        }
    }
}