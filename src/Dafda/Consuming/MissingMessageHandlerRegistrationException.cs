using System;

namespace Dafda.Consuming
{
    public sealed class MissingMessageHandlerRegistrationException : Exception
    {
        public MissingMessageHandlerRegistrationException(string message) : base(message)
        {
            
        }
    }
}