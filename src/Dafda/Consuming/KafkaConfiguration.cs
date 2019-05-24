using System;
using System.Collections.Generic;
using System.Linq;

namespace Dafda.Consuming
{
    public class KafkaConfiguration
    {
        private readonly string _keyPrefix;
        private readonly IConfigurationProvider _configuration;

        public KafkaConfiguration(IConfigurationProvider configuration, string keyPrefix = "")
        {
            _configuration = configuration;
            _keyPrefix = keyPrefix;
        }

        private string Key(string keyName) => string.Join("", _keyPrefix, keyName.ToUpper().Replace('.', '_'));

        private Tuple<string, string> GetConfiguration(string key)
        {
            var value = _configuration.GetByKey(Key(key));

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Tuple.Create(key, value);
        }

        public Dictionary<string, string> GetConfiguration()
        {
            var configurationKeys = new[]
            {
                "group.id",
                "enable.auto.commit",
                "bootstrap.servers",
                "broker.version.fallback",
                "api.version.fallback.ms",
                "ssl.ca.location",
                "sasl.username",
                "sasl.password",
                "sasl.mechanisms",
                "security.protocol",
            };

            //config.Add(new KeyValuePair<string, object>("request.timeout.ms", "3000"));

            return configurationKeys
                .Select(key => GetConfiguration(key))
                .Where(pair => pair != null)
                .ToDictionary(pair => pair.Item1, pair => pair.Item2);
        }
    }
}