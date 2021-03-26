using System.Text.Json;

namespace Dafda.Tests
{
    public static class ObjectExtensions
    {
        public static string SerializeAsJson<T>(this T o)
        {
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = false,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            return JsonSerializer.Serialize(o, options);
        }
    }
}