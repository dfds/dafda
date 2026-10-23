using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// Thrown when no implementation of <see cref="IMessageHandler{T}"/> was supplied
    /// to the <see cref="IHandlerUnitOfWork.Run"/> method, or when the message handler
    /// could not be created.
    /// </summary>
    public sealed class InvalidMessageHandlerException : Exception
    {
        /// <summary>Exception constructor</summary>
        public InvalidMessageHandlerException(string message) : base(message)
        {
        }

        /// <summary>Exception constructor</summary>
        public InvalidMessageHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}