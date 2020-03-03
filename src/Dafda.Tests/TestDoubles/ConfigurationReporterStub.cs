using Dafda.Configuration;

namespace Dafda.Tests.TestDoubles
{
    internal class ConfigurationReporterStub : ConfigurationReporter
    {
        private readonly string _reportContent;

        public ConfigurationReporterStub(string reportContent)
        {
            _reportContent = reportContent;
        }

        public override void AddMissing(string key, string source, params string[] attemptedKeys)
        {
        }

        public override void AddValue(string key, string source, string value, string acceptedKey)
        {
        }

        public override void AddManual(string key, string value)
        {
        }

        public override string Report()
        {
            return _reportContent;
        }
    }
}