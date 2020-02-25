using System;

namespace Dafda.Producing
{
    public sealed class ProducerFactoryException : Exception
    {
        public ProducerFactoryException(string message) : base(message)
        {
            
        }
    }
}