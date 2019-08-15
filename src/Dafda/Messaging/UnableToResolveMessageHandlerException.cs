using System;

namespace Dafda.Messaging
{
    public class UnableToResolveMessageHandlerException : Exception
    {
        public UnableToResolveMessageHandlerException(string message) : base(message)
        {
            
        }
    }
}