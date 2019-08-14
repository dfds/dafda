using System.Collections.Generic;

namespace Dafda.Configuration
{
    public class ConsumerConfigurationBuilder : ConfigurationBuilder
    {
        private static readonly string[] RequiredConfigurationKeys =
        {
            ConfigurationProperties.GroupId,
            ConfigurationProperties.BootstrapServers
        };

        protected override IEnumerable<string> GetRequiredConfigurationKeys()
        {
            return RequiredConfigurationKeys;
        }
    }
}