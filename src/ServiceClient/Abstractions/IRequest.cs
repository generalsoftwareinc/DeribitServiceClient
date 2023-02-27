﻿using System.Text.Json.Serialization;

namespace Deribit.ServiceClient.Abstractions
{
    public interface IRequest<T>
    {
        public long Id { get; set; }

        [JsonPropertyName("jsonrpc")]
        public string JsonRpcVersion { get; set; }

        public string Method { get; set; }

        [JsonPropertyName("params")]
        public T? Parameters { get; set; }
    }
}
