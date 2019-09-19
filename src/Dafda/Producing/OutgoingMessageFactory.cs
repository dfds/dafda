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

    internal class OutgoingMessageFactory
    {
        private readonly IMessageIdGenerator _messageIdGenerator;

        public OutgoingMessageFactory() : this(new DefaultMessageIdGenerator())
        {
        }

        public OutgoingMessageFactory(IMessageIdGenerator messageIdGenerator)
        {
            _messageIdGenerator = messageIdGenerator;
        }

        public OutgoingMessage Create<TMessage>(TMessage msg) where TMessage : IMessage
        {
            var (topicName, type) = GetMessageMetaData(msg);

            var messageId = _messageIdGenerator.NextMessageId();
            var key = msg.AggregateId;
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