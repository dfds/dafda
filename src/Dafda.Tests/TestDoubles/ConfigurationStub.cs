using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Dafda.Tests.TestDoubles
{
    public class ConfigurationStub : Microsoft.Extensions.Configuration.IConfiguration
    {
        private readonly IDictionary<string, string> _configuration;

        public ConfigurationStub(params (string key, string value)[] configuration)
        {
            _configuration = configuration.ToDictionary(x => x.key, x => x.value);
        }

        IConfigurationSection Microsoft.Extensions.Configuration.IConfiguration.GetSection(string key)
        {
            throw new System.NotImplementedException();
        }

        IEnumerable<IConfigurationSection> Microsoft.Extensions.Configuration.IConfiguration.GetChildren()
        {
            throw new System.NotImplementedException();
        }

        IChangeToken Microsoft.Extensions.Configuration.IConfiguration.GetReloadToken()
        {
            throw new System.NotImplementedException();
        }

        public string this[string key]
        {
            get
            {
                _configuration.TryGetValue(key, out var value);
                return value;
            }
            set => _configuration[key] = value;
        }
    }
}