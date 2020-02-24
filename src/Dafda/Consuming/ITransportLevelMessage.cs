using System;
using System.Linq;
using System.Text.Json;

namespace Dafda.Consuming
{
    public interface ITransportLevelMessage
    {
        Metadata Metadata { get; }

        object ReadDataAs(Type messageInstanceType);
    }

    public interface IIncomingMessageFactory
    {
        ITransportLevelMessage Create(string rawMessage);
    }

    internal class JsonIncomingMessageFactory : IIncomingMessageFactory
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ITransportLevelMessage Create(string rawMessage)
        {
            var jsonDocument = JsonDocument.Parse(rawMessage);

            var dataProperty = jsonDocument.RootElement.GetProperty(MessageEnvelopeProperties.Data);
            var jsonData = dataProperty.GetRawText();

            var metadataProperties = jsonDocument
                .RootElement
                .EnumerateObject()
                .Where(property => property.Name != MessageEnvelopeProperties.Data)
                .ToDictionary(x => x.Name, x => x.Value.GetString());

            return new TransportLevelMessage(new Metadata(metadataProperties), type => JsonSerializer.Deserialize(jsonData, type, JsonSerializerOptions));
        }
    }

    public sealed class TransportLevelMessage : ITransportLevelMessage
    {
        private readonly Func<Type, object> _deserializer;

        public TransportLevelMessage(Metadata metadata, Func<Type, object> deserializer)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        public Metadata Metadata { get; }

        public object ReadDataAs(Type messageInstanceType)
        {
            return _deserializer(messageInstanceType);
        }
    }
}