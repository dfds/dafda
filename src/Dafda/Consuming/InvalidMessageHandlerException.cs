using System;

namespace Dafda.Consuming
{
    public sealed class InvalidMessageHandlerException : Exception
    {
        public InvalidMessageHandlerException(string message) : base(message)
        {
        }
    }
}