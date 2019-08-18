using System;

namespace Dafda.Configuration
{
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message) : base(message)
        {
        }
    }
}