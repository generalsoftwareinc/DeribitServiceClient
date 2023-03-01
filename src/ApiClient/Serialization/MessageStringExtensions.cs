using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deribit.ApiClient.Serialization
{
    internal static class MessageStringExtensions
    {
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = new LowerCaseJsonNamingPolicy(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static bool TryDeserialize<T>(this string message, out T? data)
            where T : class
        {
            data = default;

            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            try
            {
                data = JsonSerializer.Deserialize<T>(message, jsonOptions);
                return data != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
