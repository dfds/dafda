using System;

namespace Dafda.Producing
{
    internal class OutgoingMessageRegistration
    {
        public OutgoingMessageRegistration(string topic, string type, Func<object, string> keySelector, string version)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(topic));
            }
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(type));
            }

            Type = type;
            Topic = topic;
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            Version = version;
        }

        public string Type { get; }
        public string Topic { get; }
        public string Version { get; }
        public Func<object, string> KeySelector { get; }
    }
}