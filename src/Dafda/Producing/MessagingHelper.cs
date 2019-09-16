using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dafda.Producing
{
    internal static class MessagingHelper
    {
        public static string CreateMessageFrom(string type, object data)
        {
            var message = new
            {
                MessageId = Guid.NewGuid(),
                Type = type,
                Data = data
            };

            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
            });
        }
    }
}