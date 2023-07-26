using System;

namespace Dafda.Configuration
{
    /// <summary>
    /// Thrown when the minimum required configuration for Kafka is not met;
    /// for <i>Producers</i> the minimum configuration is:
    /// 
    /// <list type="bullet">
    ///     <item>
    ///         <description>bootstrap.servers</description>
    ///     </item>
    /// </list>
    /// 
    /// and for <i>Consumers</i> the minimum configuration is:
    /// 
    /// <list type="bullet">
    ///     <item>
    ///         <description>bootstrap.servers</description>
    ///     </item>
    ///     <item>
    ///         <description>group.id</description>
    ///     </item>
    /// </list>
    /// </summary>
    public sealed class InvalidConfigurationException : Exception
    {
        /// <summary>Exception constructor</summary>
        public InvalidConfigurationException(string message) : base(message)
        {
        }
    }
}