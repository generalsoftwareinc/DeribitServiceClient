using System.Text.Json;

namespace Deribit.ApiClient.Serialization
{
    internal class LowerCaseJsonNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLower();
    }
}
