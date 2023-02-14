using System.Text.Json;

namespace ServiceClient.Implements
{
    internal class LowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLower();
    }
}
