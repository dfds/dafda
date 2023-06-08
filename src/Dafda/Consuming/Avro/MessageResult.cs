using Avro.Specific;
using Confluent.Kafka;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dafda.Consuming.Avro
{
    /// <summary>
    /// Result on consumed message wrapper, with key, value, headers and metadata
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MessageResult<TKey, TValue> where TValue : ISpecificRecord
    {
        private static readonly Func<Task> EmptyCommitAction = () => Task.CompletedTask;
        private readonly Func<Task> _onCommit;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="headers"></param>
        /// <param name="metadata"></param>
        /// <param name="onCommit"></param>
        public MessageResult(TKey key, TValue value, IEnumerable<KeyValuePair<string, byte[]>> headers, MessageMetadata metadata, Func<Task> onCommit = null)
        {
            _onCommit = onCommit ?? EmptyCommitAction;
            Key = key;
            Value = value;
            Headers = headers;
            Metadata = metadata;
        }

        /// <summary>
        /// 
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Headers for the message
        /// </summary>
        public IEnumerable<KeyValuePair<string, byte[]>> Headers { get; set; }

        /// <summary>
        /// Metadata for the consumed message
        /// </summary>
        public MessageMetadata Metadata { get; set; }

        internal async Task Commit()
        {
            await _onCommit();
        }
    }
}
