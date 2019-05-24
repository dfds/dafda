using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;

namespace Dafda.Consuming
{
    public class KafkaConsumerFactory
    {
        private readonly KafkaConfiguration _configuration;

        public KafkaConsumerFactory(KafkaConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConsumer<string, string> Create(params KeyValuePair<string, string>[] configOverrides)
        {
            var config = _configuration.GetConfiguration();
            
            foreach (var pair in configOverrides)
            {
                var key = pair.Key;
                var value = pair.Value;
                
                if (config.ContainsKey(key))
                {
                    config[key] = value;
                }
                else
                {
                    config.Add(key, value);
                }
            }
            
            return new ConsumerBuilder<string, string>(config.AsEnumerable()).Build();
        }
    }
}