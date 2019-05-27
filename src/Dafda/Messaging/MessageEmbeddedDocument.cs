using System;
using Newtonsoft.Json.Linq;

namespace Dafda.Messaging
{
    public class MessageEmbeddedDocument
    {
        private readonly JObject _jObject;

        public MessageEmbeddedDocument(string json)
        {
            _jObject = JObject.Parse(json);
        }

        public string MessageId => _jObject.SelectToken("messageId")?.Value<string>();
        public string Type => _jObject.SelectToken("type")?.Value<string>();
        public string CorrelationId => _jObject.SelectToken("correlationId")?.Value<string>();

        public T ReadDataAs<T>() where T : class, new()
        {
            return (T) ReadDataAs(typeof(T));
        }

        public object ReadDataAs(Type messageInstanceType)
        {
            return _jObject
                .SelectToken("data")
                .ToObject(messageInstanceType);
        }
    }
}