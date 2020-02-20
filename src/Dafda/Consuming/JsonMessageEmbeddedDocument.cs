using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Dafda.Consuming
{
    public class JsonMessageEmbeddedDocument : ITransportLevelMessage
    {
        private readonly JsonDocument _jObject;

        public JsonMessageEmbeddedDocument(string json)
        {
            _jObject = JsonDocument.Parse(json);

            Metadata = new Metadata(
                _jObject
                    .RootElement
                    .EnumerateObject()
                    .Where(property => property.Name != "data")
                    .ToDictionary(x => x.Name, x => x.Value.GetString())
            );
        }

        public Metadata Metadata { get; }

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

    /// <remarks>
    /// https://blog.arkency.com/correlation-id-and-causation-id-in-evented-systems/
    /// https://codeopinion.com/message-properties/
    /// </remarks>
    public sealed class Metadata
    {
        private readonly IDictionary<string, string> _metadata;

        internal Metadata() : this(new Dictionary<string, string>())
        {
        }

        internal Metadata(IDictionary<string, string> metadata)
        {
            _metadata = metadata;
        }

        public string MessageId
        {
            get => this["messageId"];
            set => this["messageId"] = value;
        }

        public string Type
        {
            get => this["type"];
            set => this["type"] = value;
        }

        public string CorrelationId
        {
            get => this["correlationId"];
            set => this["correlationId"] = value;
        }

        public string CausationId
        {
            get => this["causationId"];
            set => this["causationId"] = value;
        }

        public string this[string key]
        {
            get
            {
                _metadata.TryGetValue(key, out var value);
                return value;
            }
            private set => _metadata[key] = value;
        }
    }
}