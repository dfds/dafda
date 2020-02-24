using System;

namespace Dafda.Consuming
{
    public sealed class MessageRegistrationException : Exception
    {
        internal MessageRegistrationException(string message) : base(message)
        {
        }
    }
}