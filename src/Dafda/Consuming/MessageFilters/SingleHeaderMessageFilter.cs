using System;
using System.Linq;
using System.Collections.Generic;
using Dafda.Configuration;

namespace Dafda.Consuming.MessageFilters
{
    /// <inheritdoc />
    public class SingleHeaderMessageFilter : MessageFilter
    {
        private readonly string _referenceHeaderName;
        private readonly string _requiredHeaderValue;

        /// <summary>
        /// Evaluates provided header and header value for each consumed message to determine whether to post to configured handler or to disregard
        /// </summary>
        /// <param name="headerName">Header name to be filtered on</param>
        /// <param name="headerValue">Required value of provided header name</param>
        public SingleHeaderMessageFilter(string headerName, string headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerName))
            {
                throw new InvalidConfigurationException("HeaderName must not be null or empty in the HeaderMessageFilter");
            }

            if (string.IsNullOrWhiteSpace(headerValue))
            {
                throw new InvalidConfigurationException("HeaderValue must not be null or empty in the HeaderMessageFilter");
            }

            _referenceHeaderName = headerName;
            _requiredHeaderValue = headerValue;
        }

        /// <inheritdoc />
        public override bool CanAcceptMessage(MessageResult result)
        {
            var header = result?.Message.Metadata.AsEnumerable()?.FirstOrDefault(i => string.Equals(i.Key, _referenceHeaderName, StringComparison.InvariantCultureIgnoreCase));

            if (header.Equals(default(KeyValuePair<string, string>)))
            {
                return false;
            }

            return header.HasValue && string.Equals(header.Value.Value, _requiredHeaderValue, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
