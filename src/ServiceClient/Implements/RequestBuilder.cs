using System.Text.Json;

namespace ServiceClient.Implements
{
    internal static class RequestBuilder
    {
        internal static string BuildRequest(Request request)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new LowerCaseNamingPolicy()
            };
            var ser = JsonSerializer.Serialize(request, jsonOptions);
            return ser;
        }
    }
}
