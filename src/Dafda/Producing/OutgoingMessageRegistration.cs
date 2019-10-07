using System;

namespace Dafda.Producing
{
    public class OutgoingMessageRegistration
    {
        public OutgoingMessageRegistration(string topic, string type, Func<object, string> keySelector)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(topic));
            }
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(type));
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            Type = type;
            Topic = topic;
            KeySelector = keySelector;
        }

        public string Type { get; }
        public string Topic { get; }
        public Func<object, string> KeySelector { get; }
    }
}