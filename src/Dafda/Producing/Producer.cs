using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Dafda.Consuming;
using Dafda.Diagnostics;

namespace Dafda.Producing
{
    /// <summary>
    /// Produce messages on Kafka
    /// </summary>
    public sealed class Producer
    {
        private readonly KafkaProducer _kafkaProducer;
        private readonly PayloadDescriptorFactory _payloadDescriptorFactory;

        internal Producer(KafkaProducer kafkaProducer, OutgoingMessageRegistry outgoingMessageRegistry, MessageIdGenerator messageIdGenerator)
        {
            _kafkaProducer = kafkaProducer;
            _payloadDescriptorFactory = new PayloadDescriptorFactory(outgoingMessageRegistry, messageIdGenerator);
        }

        internal string Name { get; set; } = "__Default Producer__";

        /// <summary>
        /// Produce multiple <paramref name="messages"/> on Kafka. The messages will be produced in the order of the IEnumerable provided.
        /// </summary>
        /// <param name="messages">The messages</param>
        /// <param name="onDelivery">Callback invoked for each message after delivery</param>
        public async Task Produce(IEnumerable<object> messages, Action<Confluent.Kafka.DeliveryResult<string, string>> onDelivery = null)
        {
            var produceTasks = messages.Select(async message =>
            {
                var result = await Produce(message);
                onDelivery?.Invoke(result);
            });
            await Task.WhenAll(produceTasks);
        }

        /// <summary>
        /// Produce multiple <paramref name="messages"/> on Kafka including <paramref name="headers"/>. The messages will be produced in the order of the IEnumerable provided.
        /// </summary>
        /// <param name="messages">The messages</param>
        /// <param name="headers">The message headers</param>
        /// <param name="onDelivery">Callback invoked for each message after delivery</param>
        public async Task Produce(IEnumerable<object> messages, Metadata headers, Action<Confluent.Kafka.DeliveryResult<string, string>> onDelivery = null)
        {
            var produceTasks = messages.Select(async message =>
            {
                var result = await Produce(message, headers);
                onDelivery?.Invoke(result);
            });
            await Task.WhenAll(produceTasks);
        }

        /// <summary>
        /// Produce multiple <paramref name="messages"/> on Kafka including <paramref name="headers"/>. The messages will be produced in the order of the IEnumerable provided.
        /// </summary>
        /// <param name="messages">The messages</param>
        /// <param name="headers">The message headers</param>
        /// <param name="onDelivery">Callback invoked for each message after delivery</param>
        public async Task Produce(IEnumerable<object> messages, Dictionary<string, object> headers, Action<Confluent.Kafka.DeliveryResult<string, string>> onDelivery = null)
        {
            var produceTasks = messages.Select(async message =>
            {
                var result = await Produce(message, headers);
                onDelivery?.Invoke(result);
            });
            await Task.WhenAll(produceTasks);
        }

        /// <summary>
        /// Produce multiple <paramref name="messages"/> on Kafka. The messages will be produced in the order of the IEnumerable provided.
        /// </summary>
        /// <param name="messages">The messages</param>
        /// <param name="context">Context from the consumer. Supply this to get correlation and causation id on the new messages</param>
        /// <param name="onDelivery">Callback invoked for each message after delivery</param>
        public async Task Produce(IEnumerable<object> messages, MessageHandlerContext context, Action<Confluent.Kafka.DeliveryResult<string, string>> onDelivery = null)
        {
            var produceTasks = messages.Select(async message =>
            {
                var result = await Produce(message, context);
                onDelivery?.Invoke(result);
            });
            await Task.WhenAll(produceTasks);
        }

        /// <summary>
        /// Produce multiple <paramref name="messages"/> on Kafka. The messages will be produced in the order of the IEnumerable provided.
        /// </summary>
        /// <param name="messages">The messages</param>
        /// <param name="context">Context from the consumer. Supply this to get correlation and causation id on the new messages</param>
        /// <param name="headers">Additional message headers</param>
        /// <param name="onDelivery">Callback invoked for each message after delivery</param>
        public async Task Produce(IEnumerable<object> messages, MessageHandlerContext context, Dictionary<string, string> headers, Action<Confluent.Kafka.DeliveryResult<string, string>> onDelivery = null)
        {
            var produceTasks = messages.Select(async message =>
            {
                var result = await Produce(message, context, headers);
                onDelivery?.Invoke(result);
            });
            await Task.WhenAll(produceTasks);
        }

        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>The delivery result from Kafka</returns>
        public async Task<Confluent.Kafka.DeliveryResult<string, string>> Produce(object message)
        {
            return await Produce(message, new Metadata());
        }

        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="headers">The message headers</param>
        /// <returns>The delivery result from Kafka</returns>
        public async Task<Confluent.Kafka.DeliveryResult<string, string>> Produce(object message, Metadata headers)
        {
            var payloadDescriptor = _payloadDescriptorFactory.Create(message, headers);
            payloadDescriptor.ClientId = _kafkaProducer.ClientId;
            using var activity = DafdaActivitySource.StartPublishingActivity(payloadDescriptor);

            return await _kafkaProducer.Produce(payloadDescriptor);
        }

        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka including <paramref name="headers"/>
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="headers">The message headers</param>
        /// <returns>The delivery result from Kafka</returns>
        public async Task<Confluent.Kafka.DeliveryResult<string, string>> Produce(object message, Dictionary<string, object> headers)
        {
            var dict = headers.ToDictionary( pair => pair.Key, pair => pair.Value.ToString());
            return await Produce(message, new Metadata( dict ));
        }

        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="context">Context from the consumer. Supply this to get correlation and causation id on the new message</param>
        /// <returns>The delivery result from Kafka</returns>
        public async Task<Confluent.Kafka.DeliveryResult<string, string>> Produce(object message, MessageHandlerContext context)
        {
            return await Produce(message, context, new Dictionary<string, string>());
        }

        /// <summary>
        /// Produce a <paramref name="message"/> on Kafka
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="context">Context from the consumer. Supply this to get correlation and causation id on the new message</param>
        /// <param name="headers">Additional message headers</param>
        /// <returns>The delivery result from Kafka</returns>
        public async Task<Confluent.Kafka.DeliveryResult<string, string>> Produce(object message, MessageHandlerContext context, Dictionary<string, string> headers)
        {
            var payloadDescriptor = _payloadDescriptorFactory.Create(message, context, headers);
            payloadDescriptor.ClientId = _kafkaProducer.ClientId;
            using var activity = DafdaActivitySource.StartPublishingActivity(payloadDescriptor);

            return await _kafkaProducer.Produce(payloadDescriptor);
        }
    }
}