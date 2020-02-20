using System;

namespace Dafda.Consuming
{
    public class InvalidMessageHandlerException : Exception
    {
        public InvalidMessageHandlerException(string message) : base(message)
        {
        }
    }
}