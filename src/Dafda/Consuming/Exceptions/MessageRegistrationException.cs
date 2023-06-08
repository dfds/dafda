using System;
using Dafda.Consuming.Interfaces;

namespace Dafda.Consuming
{
    /// <summary>
    /// Thrown when trying to register any type that does not implement <see cref="IMessageHandler{T}"/>.
    /// </summary>
    public sealed class MessageRegistrationException : Exception
    {
        internal MessageRegistrationException(string message) : base(message)
        {
        }
    }
}