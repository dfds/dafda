using System;
using System.Linq;
using Dafda.Configuration;

namespace Dafda.Consuming.MessageFilters
{
    /// <inheritdoc />
    public class SingleHeaderMessageFilter : MessageFilter
    {
        private readonly string _referenceHeaderName;
        private readonly string _requiredHeaderValue;

        /// <inheritdoc />
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
            if (result?.Message != null)
            {
               var header = result.Message.Metadata.AsEnumerable()?.FirstOrDefault(i => string.Equals(i.Key, _referenceHeaderName, StringComparison.InvariantCultureIgnoreCase));

               if (header != null && header.HasValue && header.Value.Value != null && string.Equals(header.Value.Value, _requiredHeaderValue, StringComparison.InvariantCultureIgnoreCase))
               {
                   return true;
               }
            }

            return false;
        }
    }
}
