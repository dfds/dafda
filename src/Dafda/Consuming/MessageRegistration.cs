using System;

namespace Dafda.Consuming
{
    /// <summary>
    /// A configuration type for linking a topic and message type to a message handler
    /// </summary>
    public sealed class MessageRegistration
    {
        /// <summary>
        /// Create a message registration with the given properties,
        /// will throw if the handler doesn't match the message
        /// </summary>
        public MessageRegistration(
            Type handlerInstanceType,
            Type messageInstanceType,
            string topic,
            string messageType)
        {
            EnsureProperHandlerType(handlerInstanceType, messageInstanceType);

            HandlerInstanceType = handlerInstanceType;
            MessageInstanceType = messageInstanceType;
            MessageType = messageType;
            Topic = EnsureValidTopicName(topic);
        }

        private static string EnsureValidTopicName(string topicName)
        {
            // Passing a null topic, will cause Confluent to throw an AccessViolationException, which cannot be caught by the service, resulting in a hard crash without logs.
            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException(nameof(topicName), "Topic must have a value");
            }

            return topicName;
        }

        private static void EnsureProperHandlerType(Type handlerInstanceType, Type messageInstanceType)
        {
            var expectedHandlerInstanceBaseType = typeof(IMessageHandler<>).MakeGenericType(messageInstanceType);
            if (expectedHandlerInstanceBaseType.IsAssignableFrom(handlerInstanceType))
            {
                return;
            }

            var openGenericInterfaceName = typeof(IMessageHandler<>).Name;
            var expectedInterface = $"{openGenericInterfaceName.Substring(0, openGenericInterfaceName.Length - 2)}<{messageInstanceType.FullName}>";

            throw new MessageRegistrationException($"Error! Message handler type \"{handlerInstanceType.FullName}\" does not implement expected interface \"{expectedInterface}\". It's expected when registered together with a message instance type of \"{messageInstanceType.FullName}\".");
        }

        /// <summary>The type of the message handler</summary>
        public Type HandlerInstanceType { get; }
        /// <summary>The type of the message</summary>
        public Type MessageInstanceType { get; }
        /// <summary>The name of the kafka topic</summary>
        public string Topic { get; }
        /// <summary>The name of the message type as sent over kafka</summary>
        public string MessageType { get; }
    }
}
