using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dafda.Logging;

namespace Dafda.Configuration
{
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message) : base(message)
        {
        }
    }
}