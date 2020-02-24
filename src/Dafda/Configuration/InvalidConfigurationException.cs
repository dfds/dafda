using System;

namespace Dafda.Configuration
{
    public sealed class InvalidConfigurationException : Exception
    {
        internal InvalidConfigurationException(string message) : base(message)
        {
        }
    }
}