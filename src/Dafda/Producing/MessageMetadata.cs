using System;
using System.Reflection;

namespace Dafda.Producing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageAttribute : Attribute
    {
        public MessageAttribute(string topic, string type)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(topic));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(type));
            }

            Topic = topic;
            Type = type;
        }

        public string Topic { get; }
        public string Type { get; }
    }

    public interface IMessage
    {
        string AggregateId { get; }
    }

    internal class MessageMetadata
    {
        public static MessageMetadata Create<TMessage>(TMessage msg) where TMessage : IMessage
        {
            var messageAttribute = msg.GetType()
                .GetTypeInfo()
                .GetCustomAttribute<MessageAttribute>();

            if (messageAttribute == null)
            {
                throw new InvalidOperationException($@"Message ""{typeof(TMessage).Name}"" must have a ""{nameof(MessageAttribute)}"" declared.");
            }

            return new MessageMetadata(messageAttribute.Topic, messageAttribute.Type);
        }

        public MessageMetadata(string topicName, string type)
        {
            TopicName = topicName;
            Type = type;
        }

        public string TopicName { get; }
        public string Type { get; }
    }
}