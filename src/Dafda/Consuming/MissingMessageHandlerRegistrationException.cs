using System;

namespace Dafda.Consuming
{
    public class MissingMessageHandlerRegistrationException : Exception
    {
        public MissingMessageHandlerRegistrationException(string message) : base(message)
        {
            
        }
    }
}