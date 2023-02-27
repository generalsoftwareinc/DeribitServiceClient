using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Deribit.ServiceClient.DTOs.Subscribe
{
    internal record SubscribeRequest
    {
        [JsonPropertyName("channels")]
        public string[] Channels { get; init; }
    }
}
