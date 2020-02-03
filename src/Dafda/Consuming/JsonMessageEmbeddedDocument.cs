using System;
using System.Text.Json;

namespace Dafda.Consuming
{
    public class JsonMessageEmbeddedDocument : ITransportLevelMessage
    {
        private readonly JsonDocument _jObject;

        public JsonMessageEmbeddedDocument(string json)
        {
            _jObject = JsonDocument.Parse(json);
        }

        public string MessageId => _jObject.RootElement.GetProperty("messageId").GetString();
        public string Type => _jObject.RootElement.GetProperty("type").GetString();
        public string CorrelationId => _jObject.RootElement.GetProperty("correlationId").GetString();

        public T ReadDataAs<T>() where T : class, new()
        {
            return (T) ReadDataAs(typeof(T));
        }

        public object ReadDataAs(Type messageInstanceType)
        {
            var element = _jObject.RootElement.GetProperty("data");
            var json = element.GetRawText();
            return JsonSerializer.Deserialize(
                json,
                messageInstanceType, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }
    }
}