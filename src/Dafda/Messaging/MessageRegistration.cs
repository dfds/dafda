using System;

namespace Dafda.Messaging
{
    public class MessageRegistration
    {
        public MessageRegistration(Type handlerInstanceType, Type messageInstanceType, string topic, string messageType)
        {
            HandlerInstanceType = handlerInstanceType;
            MessageInstanceType = messageInstanceType;
            Topic = topic;
            MessageType = messageType;
        }

        public Type HandlerInstanceType { get; }
        public Type MessageInstanceType { get; }
        public string Topic { get; }
        public string MessageType { get; }
    }
}