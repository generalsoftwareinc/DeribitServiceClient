using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Deribit.ServiceClient.DTOs;

namespace Deribit.ServiceClient.Serialization
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
