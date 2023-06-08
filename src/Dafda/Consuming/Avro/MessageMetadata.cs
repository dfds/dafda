using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dafda.Consuming.Avro
{
    /// <summary>
    /// Metadata for the consumed message
    /// </summary>
    public class MessageMetadata
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="partition"></param>
        /// <param name="timestamp"></param>
        /// <param name="offset"></param>
        public MessageMetadata(string topic, int partition, Timestamp timestamp, long offset)
        {
            Topic = topic;
            Partition = partition;
            Timestamp = timestamp;
            Offset = offset;
        }
        /// <summary>
        /// Topic for the consumed message
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// Partition for the consumed message
        /// </summary>
        public int Partition { get; set; }
        /// <summary>
        /// Timestamp for the consumed message
        /// </summary>
        public Timestamp Timestamp { get; set; }

        /// <summary>
        /// Offset for the consumed message
        /// </summary>
        public long Offset { get; set; }
    }
}
