using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// Thrown when trying to register any type that does not implement <see cref="IMessageHandler{T}"/>.
    /// </summary>
    public sealed class MessageRegistrationException : Exception
    {
        public MessageRegistrationException(string message) : base(message)
        {
        }
    }
}