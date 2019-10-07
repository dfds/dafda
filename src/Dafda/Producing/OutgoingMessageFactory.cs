using System;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dafda.Producing
{
    public interface IMessageIdGenerator
    {
        string NextMessageId();
    }

    internal class DefaultMessageIdGenerator : IMessageIdGenerator
    {
        public string NextMessageId()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class OutgoingMessageFactory
    {
        private readonly IMessageIdGenerator _messageIdGenerator;
        private readonly IOutgoingMessageRegistry _outgoingMessageRegistry;

        public OutgoingMessageFactory() : this(new DefaultMessageIdGenerator(), new OutgoingMessageRegistry())
        {
        }

        public OutgoingMessageFactory(IMessageIdGenerator messageIdGenerator, IOutgoingMessageRegistry outgoingMessageRegistry)
        {
            _messageIdGenerator = messageIdGenerator;
            _outgoingMessageRegistry = outgoingMessageRegistry;
        }

        public OutgoingMessage Create<TMessage>(TMessage msg)
        {
            string topicName;
            string type;
            string key;

            if (msg is IMessage message)
            {
                (topicName, type) = GetMessageMetaData(message);

                key = message.AggregateId;
            }
            else
            {
                var registration = _outgoingMessageRegistry.GetRegistration(msg);
                topicName = registration.Topic;
                type = registration.Type;
                key = registration.KeySelector(msg);
            }

            var messageId = _messageIdGenerator.NextMessageId();

            var rawMessage = CreateRawMessage(messageId, type, msg);

            return new OutgoingMessage(topicName, messageId, key, type, rawMessage);
        }

        private static (string topicName, string type) GetMessageMetaData<TMessage>(TMessage msg) where TMessage : IMessage
        {
            var messageAttribute = msg.GetType()
                .GetTypeInfo()
                .GetCustomAttribute<MessageAttribute>();

            if (messageAttribute == null)
            {
                throw new InvalidOperationException($@"Message ""{typeof(TMessage).Name}"" must have a ""{nameof(MessageAttribute)}"" declared.");
            }

            return (messageAttribute.Topic, messageAttribute.Type);
        }

        private static string CreateRawMessage(string messageId, string type, object data)
        {
            var message = new
            {
                MessageId = messageId,
                Type = type,
                Data = data
            };

            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
        }
    }
}