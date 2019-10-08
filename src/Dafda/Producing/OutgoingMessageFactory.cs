using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dafda.Producing
{
    public class OutgoingMessageFactory
    {
        private readonly MessageIdGenerator _messageIdGenerator;
        private readonly IOutgoingMessageRegistry _outgoingMessageRegistry;

        public OutgoingMessageFactory(MessageIdGenerator messageIdGenerator, IOutgoingMessageRegistry outgoingMessageRegistry)
        {
            _messageIdGenerator = messageIdGenerator;
            _outgoingMessageRegistry = outgoingMessageRegistry;
        }

        public OutgoingMessage Create(object msg)
        {
            string topicName;
            string type;
            string key;

            if (msg is IMessage message)
            {
                var messageMetadata = MessageMetadata.Create(message);
                topicName = messageMetadata.TopicName;
                type = messageMetadata.Type;
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