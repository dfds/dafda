using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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

            ActivityContext context = default;
            Activity activity = Activity.Current;
            if (activity != null)
            {
                context = activity.Context;
            }

            Propagator.Inject(new PropagationContext(context, Baggage.Current), envelope, InjectMetadata);

            activity?.SetTag("messaging.system", "kafka");
            activity?.SetTag("messaging.destination", descriptor.TopicName);
            activity?.SetTag("messaging.destination_kind", "topic");
            activity?.SetTag("messaging.message_id", descriptor.MessageId);
            //activity?.SetTag("messaging.conversation_id", descriptor.CorrelationId);
            //activity?.SetTag("messaging.message_payload_size_bytes", "0");
            // consumer
            // kafka
            activity?.SetTag("messaging.kafka.message_key", descriptor.PartitionKey);
            //activity?.SetTag("messaging.kafka.consumer_group", "consumer_group");
            //activity?.SetTag("messaging.kafka.client_id", "client_id");
            //activity?.SetTag("messaging.kafka.partition", "partition");

            envelope.Add(MakeKeyFrom("Data"), descriptor.MessageData);

            return envelope;
        }

        private static void InjectMetadata(Dictionary<string, object> headers, string key, string value)
        {
            headers[key] = value;
        }


        internal static TextMapPropagator Propagator { get; set; }= Propagators.DefaultTextMapPropagator;
    }
}