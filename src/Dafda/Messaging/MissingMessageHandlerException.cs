using System;

namespace Dafda.Messaging
{
    public class MissingMessageHandlerException : Exception
    {
        public MissingMessageHandlerException(string message) : base(message)
        {
            
        }
    }
}