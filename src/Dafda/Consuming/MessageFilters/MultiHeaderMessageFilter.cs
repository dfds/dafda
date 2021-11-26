using System;
using System.Collections.Generic;
using System.Linq;
using Dafda.Configuration;
using Dafda.Serializing;

namespace Dafda.Consuming.MessageFilters
{
    /// <inheritdoc />
    public class MultiHeaderMessageFilter : MessageFilter
    {
        private readonly Dictionary<string, string> _headers;
        private readonly bool _matchAll;

        /// <summary>
        /// Evaluates target headers on consumed message to determine whether to disregard message.
        /// </summary>
        /// <param name="headers">Headers to filter on for message consumption.</param>
        /// <param name="matchAll">Defaulted to false. False = Only one header and value need to match one in the message. True = All headers and values need to match those from the message.</param>
        public MultiHeaderMessageFilter(Dictionary<string, string> headers, bool matchAll = false)
        {
            if (headers.Any(x => string.IsNullOrWhiteSpace(x.Key) || string.IsNullOrWhiteSpace(x.Value)))
            {
                throw new InvalidConfigurationException("All Headers and Values must not be null or empty");
            }

            _headers = headers;
            _matchAll = matchAll;
        }

        /// <inheritdoc />
        public override bool CanAcceptMessage(MessageResult result)
        {
            if (result?.Message != null)
            {
                var messageHeaders = result.Message.Metadata.AsEnumerable().ToDictionary(k => k.Key, v => v.Value);

                if (_matchAll)
                {
                    return _headers.All(header => EvaluateKeyValuePair(messageHeaders, header));
                }
                else
                {
                    return _headers.Any(header => EvaluateKeyValuePair(messageHeaders, header));
                }
            }

            return false;
        }

        private bool EvaluateKeyValuePair(Dictionary<string, string> headers, KeyValuePair<string, string> header)
        {
            var targetHeader = headers.FirstOrDefault(x => x.Key.Equals(header.Key, StringComparison.InvariantCultureIgnoreCase));

            if (targetHeader.Equals(default(KeyValuePair<string, string>)))
            {
                return false;
            }

            return string.Equals(targetHeader.Value, header.Value, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}