using System.Collections.Generic;
using System.Text.Json.Serialization;
using NuciAPI.Responses;

namespace SteamKeyActivator.Client.Models
{
    public sealed class ProductKeyResponse : SuccessResponse
    {
        [JsonPropertyName("products")]
        public IEnumerable<ProductKeyObject> ProductKeys { get; set; }
    }
}
