using System;

namespace Dafda.Consuming
{
    public sealed class InvalidMessageHandlerException : Exception
    {
        internal InvalidMessageHandlerException(string message) : base(message)
        {
        }
    }
}