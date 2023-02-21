using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServiceClient.Implements
{
    internal static class MessageStringExtensions
    {
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

        static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLower();
    }
}
