using Dafda.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace Dafda.Tests.Configuration
{
    public class TestConfigurationReporter
    {
        private static readonly string NL = System.Environment.NewLine;

        private readonly ITestOutputHelper _output;

        public TestConfigurationReporter(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Can_print_report()
        {
            var sut = ConfigurationReporter.CreateDefault();
            sut.AddMissing("key", "source", "key1", "key2");
            sut.AddValue("another key", "some other source", "value", "key1");
            sut.AddManual("group.id", "value");

            var report = sut.Report();

            _output.WriteLine(report);

            Assert.Equal(expected:
                $"{NL}" +
                $"key         source            value   keys{NL}" +
                $"------------------------------------------------{NL}" +
                $"key         source            MISSING key1, key2{NL}" +
                $"another key some other source value   key1{NL}" +
                $"group.id    MANUAL            value   {NL}",
                report);
        }
        
    }
}