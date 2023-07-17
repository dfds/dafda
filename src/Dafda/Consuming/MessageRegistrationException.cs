using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// Thrown when trying to register any type that does not implement <see cref="IMessageHandler{T}"/>.
    /// </summary>
    public sealed class MessageRegistrationException : Exception
    {
        /// <summary>
        /// Exception constructor
        /// </summary>
        /// <param name="message"></param>
        public MessageRegistrationException(string message) : base(message)
        {
        }
    }
}