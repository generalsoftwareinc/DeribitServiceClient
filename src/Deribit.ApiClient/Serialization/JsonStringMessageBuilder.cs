using System.Text.Json.Serialization;
using System.Text.Json;
using Deribit.ApiClient.DTOs;

namespace Deribit.ApiClient.Serialization
{
    internal class JsonStringMessageBuilder
    {
        static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = new LowerCaseJsonNamingPolicy(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        internal static string BuildMessage<T>(long requestId, string method, T data)
            where T : class
        {
            var request = new Request<T>
            {
                JsonRpcVersion = "2.0",
                Id = requestId,
                Method = method,
                Parameters = data,
            };

            return JsonSerializer.Serialize(request, jsonOptions);
        }
    }
}
