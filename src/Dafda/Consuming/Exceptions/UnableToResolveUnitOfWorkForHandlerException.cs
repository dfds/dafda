using System;
using Dafda.Consuming.Interfaces;

namespace Dafda.Consuming.Exceptions
{
    /// <summary>
    /// Thrown when no <see cref="IHandlerUnitOfWork"/> was returned by
    /// <see cref="IHandlerUnitOfWorkFactory.CreateForHandlerType"/>.
    /// </summary>
    public sealed class UnableToResolveUnitOfWorkForHandlerException : Exception
    {
        internal UnableToResolveUnitOfWorkForHandlerException(string message) : base(message)
        {
        }
    }
}