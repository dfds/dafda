using System.Text;
using Confluent.Kafka;

namespace Dafda.Producing
{
    internal static class MessageFactory
    {
        public const string MessageIdHeaderName = "messageId";
        public const string TypeHeaderName = "type";

        public static Message<string, string> Create(OutgoingMessage outgoingMessage)
        {
            return new Message<string, string>
            {
                Key = outgoingMessage.Key,
                Headers = new Headers
                {
                    {MessageIdHeaderName, Encoding.ASCII.GetBytes(outgoingMessage.MessageId)},
                    {TypeHeaderName, Encoding.ASCII.GetBytes(outgoingMessage.Type)}
                },
                Value = outgoingMessage.Value
            };
        }
    }
}