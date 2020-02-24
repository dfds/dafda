using System;

namespace Dafda.Consuming
{
    public sealed class MissingMessageHandlerRegistrationException : Exception
    {
        internal MissingMessageHandlerRegistrationException(string message) : base(message)
        {
        }
    }
}