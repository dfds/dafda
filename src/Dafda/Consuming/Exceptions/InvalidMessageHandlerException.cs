using System;
using Dafda.Consuming.Interfaces;

namespace Dafda.Consuming.Exceptions
{
    /// <summary>
    /// Thrown when no implementation of <see cref="IMessageHandler{T}"/> was supplied
    /// to the <see cref="IHandlerUnitOfWork.Run"/> method.
    /// </summary>
    public sealed class InvalidMessageHandlerException : Exception
    {
        internal InvalidMessageHandlerException(string message) : base(message)
        {
        }
    }
}