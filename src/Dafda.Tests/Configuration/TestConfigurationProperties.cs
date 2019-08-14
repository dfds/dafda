using Dafda.Configuration;
using Xunit;

namespace Dafda.Tests.Configuration
{
    public class TestConfigurationProperties
    {
        [Theory]
        [InlineData("group.id", ConfigurationProperties.GroupId)]
        [InlineData("bootstrap.servers", ConfigurationProperties.BootstrapServers)]
        [InlineData("enable.auto.commit", ConfigurationProperties.EnableAutoCommit)]
        public void Has_correct_property_name(string expected, string actual)
        {
            Assert.Equal(expected, actual);
        }
    }
}