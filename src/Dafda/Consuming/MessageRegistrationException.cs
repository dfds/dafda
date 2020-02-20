using System;

namespace Dafda.Consuming
{
    public sealed class MessageRegistrationException : Exception
    {
        public MessageRegistrationException(string message) : base(message)
        {
            
        }
    }
}