using System;

namespace Dafda.Messaging
{
    public class MissingMessageHandlerRegistrationException : Exception
    {
        public MissingMessageHandlerRegistrationException(string message) : base(message)
        {
            
        }
    }
}