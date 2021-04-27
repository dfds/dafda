using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dafda.Serializing
{
    /// <summary>
    /// The default <see cref="T:System.Text.Json"/> payload serializer.
    /// </summary>
    public class DefaultPayloadSerializer : IPayloadSerializer
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initialize an instance of the class
        /// </summary>
        public DefaultPayloadSerializer()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = false,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        /// <inheritdoc />
        public string PayloadFormat { get; } = "application/json";

        /// <inheritdoc />
        public Task<string> Serialize(PayloadDescriptor payloadDescriptor)
        {
            var result = JsonSerializer.Serialize(
                value: ConvertToMessagePayload(payloadDescriptor),
                options: _jsonSerializerOptions
            );

            return Task.FromResult(result);
        }

        private object ConvertToMessagePayload(PayloadDescriptor descriptor)
        {
            // NOTE: due to a bug in system.text.json an additional conversion 
            // is made to have proper dictionary key casing - on envelope keys only though!
            // For reference: https://github.com/dotnet/runtime/issues/31849

            string MakeKeyFrom(string key) => _jsonSerializerOptions.DictionaryKeyPolicy.ConvertName(key);

            var envelope = new Dictionary<string, object>
            {
                {
                    MakeKeyFrom("MessageId"),
                    descriptor.MessageId
                },
                {
                    MakeKeyFrom("Type"),
                    descriptor.MessageType
                }
            };

            var messageHeaders = descriptor.MessageHeaders
                .Where(k => !string.Equals(k.Key, "messageId", StringComparison.InvariantCultureIgnoreCase))
                .Where(k => !string.Equals(k.Key, "type", StringComparison.InvariantCultureIgnoreCase));

            foreach (var (key, value) in messageHeaders)
            {
                envelope.Add(MakeKeyFrom(key), value);
            }

            envelope.Add(MakeKeyFrom("Data"), descriptor.MessageData);

            return envelope;
        }
    }
}