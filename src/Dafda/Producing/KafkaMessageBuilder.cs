using System.Text;
using Confluent.Kafka;

namespace Dafda.Producing
{
    internal class KafkaMessageBuilder
    {
        private static readonly Encoding Encoding = Encoding.ASCII;

        private readonly Headers _headers = new Headers();
        private string _key;
        private string _value;

        public KafkaMessageBuilder WithKey(string key)
        {
            _key = key;
            return this;
        }

        public KafkaMessageBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }

        public KafkaMessageBuilder WithHeader(string key, string value)
        {
            _headers.Add(key, Encoding.GetBytes(value));
            return this;
        }

        public Message<string, string> Build()
        {
            return new Message<string, string>
            {
                Key = _key,
                Headers = _headers,
                Value = _value
            };
        }

        public static implicit operator Message<string, string>(KafkaMessageBuilder builder)
        {
            return builder.Build();
        }
    }
}