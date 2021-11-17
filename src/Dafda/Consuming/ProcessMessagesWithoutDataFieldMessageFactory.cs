using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dafda.Consuming
{
    internal class ProcessMessagesWithoutDataFieldMessageFactory : IIncomingMessageFactory
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };

        public TransportLevelMessage Create(string rawMessage)
        {
            var jsonDocument = JsonDocument.Parse(rawMessage);

            var metadataProperties = jsonDocument
                .RootElement
                .EnumerateObject()
                .Where(property => property.Name != MessageEnvelopeProperties.Data)
                .ToDictionary(x => x.Name, x => x.Value.ToString());

            JsonElement dataProperty;
            try
            {
                dataProperty = jsonDocument.RootElement.GetProperty(MessageEnvelopeProperties.Data);
            }
            catch (KeyNotFoundException)
            {
                return new TransportLevelMessage(new Metadata(metadataProperties), type => JsonSerializer.Deserialize(jsonDocument.RootElement.GetRawText(), type, JsonSerializerOptions));
            }
            var jsonData = dataProperty.GetRawText();

            return new TransportLevelMessage(new Metadata(metadataProperties), type => JsonSerializer.Deserialize(jsonData, type, JsonSerializerOptions));
        }
    }
}
