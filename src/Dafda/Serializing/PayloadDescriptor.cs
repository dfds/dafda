using System;
using System.Collections.Generic;
using System.Linq;

namespace Dafda.Serializing
{
    /// <summary>
    /// The payload descriptor contains the <see cref="MessageData"/> as well
    /// as metadata, such as <see cref="TopicName"/>, <see cref="PartitionKey"/>,
    /// <see cref="MessageType"/>, etc.
    /// </summary>
    public sealed class PayloadDescriptor
    {
        /// <summary>
        /// Initialize an instance of the <see cref="PayloadDescriptor"/>
        /// </summary>
        /// <param name="messageId">The unique id of the message</param>
        /// <param name="topicName">The name of the topic</param>
        /// <param name="partitionKey">The partition key</param>
        /// <param name="messageType">The message type</param>
        /// <param name="messageData">The message</param>
        /// <param name="messageHeaders">A list of headers</param>
        public PayloadDescriptor(string messageId, string topicName, string partitionKey, string messageType, object messageData, IEnumerable<KeyValuePair<string, string>> messageHeaders)
        {
            MessageId = messageId;
            TopicName = topicName;
            PartitionKey = partitionKey;
            MessageType = messageType;
            MessageData = messageData;
            MessageHeaders = messageHeaders
                .Where(k => !string.Equals(k.Key, "messageId", StringComparison.InvariantCultureIgnoreCase))
                .Where(k => !string.Equals(k.Key, "type", StringComparison.InvariantCultureIgnoreCase));
    }

        /// <summary>
        /// The name of the topic
        /// </summary>
        public string TopicName { get; private set; }

        /// <summary>
        /// >The partition key
        /// </summary>
        public string PartitionKey { get; private set; }

        /// <summary>
        /// The unique id of the message
        /// </summary>
        public string MessageId { get; private set; }

        /// <summary>
        /// The message type
        /// </summary>
        public string MessageType { get; private set; }

        /// <summary>
        /// The list of headers
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> MessageHeaders { get; private set; }

        /// <summary>
        /// The message
        /// </summary>
        public object MessageData { get; private set; }
    }
}