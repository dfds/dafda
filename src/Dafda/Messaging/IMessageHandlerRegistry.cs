using System;
using System.Collections.Generic;

namespace Dafda.Messaging
{
    public interface IMessageHandlerRegistry : ITopicProvider
    {
        MessageRegistration Register(Type handlerInstanceType, Type messageInstanceType, string topic, string messageType);
        MessageRegistration Register<TMessage, THandler>(string topic, string messageType) 
            where THandler : IMessageHandler<TMessage> 
            where TMessage : class, new();

        IEnumerable<MessageRegistration> Registrations { get; }
        MessageRegistration GetRegistrationFor(string messageType);
    }
}