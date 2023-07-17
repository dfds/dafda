using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// Thrown when no implementation of <see cref="IMessageHandler{T}"/> was supplied
    /// to the <see cref="IHandlerUnitOfWork.Run"/> method.
    /// </summary>
    public sealed class InvalidMessageHandlerException : Exception
    {
        /// <summary>
        /// Exception constructor
        /// </summary>
        /// <param name="message"></param>
        public InvalidMessageHandlerException(string message) : base(message)
        {
        }
    }
}