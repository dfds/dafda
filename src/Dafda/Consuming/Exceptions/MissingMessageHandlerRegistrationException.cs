using System;
using Dafda.Consuming.Interfaces;

namespace Dafda.Consuming.Exceptions
{
    /// <summary>
    /// Thrown when no <see cref="IMessageHandler{T}"/> has been registered for a message.
    /// </summary>
    public sealed class MissingMessageHandlerRegistrationException : Exception
    {
        internal MissingMessageHandlerRegistrationException(string message) : base(message)
        {
        }
    }
}