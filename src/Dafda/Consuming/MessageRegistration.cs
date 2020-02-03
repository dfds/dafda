using System;

namespace Dafda.Consuming
{
    public class MessageRegistration
    {
        public MessageRegistration(Type handlerInstanceType, Type messageInstanceType, string topic, string messageType)
        {
            EnsureProperHandlerType(handlerInstanceType, messageInstanceType);

            HandlerInstanceType = handlerInstanceType;
            MessageInstanceType = messageInstanceType;
            Topic = topic;
            MessageType = messageType;
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

        public Type HandlerInstanceType { get; }
        public Type MessageInstanceType { get; }
        public string Topic { get; }
        public string MessageType { get; }
    }
}