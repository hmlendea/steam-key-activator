using System.Text.Json.Serialization;
using NuciSecurity.HMAC;

namespace SteamKeyActivator.Client.Models
{
    public sealed class UpdateProductKeyRequest : Request
    {
        [HmacOrder(1)]
        [JsonPropertyName("store")]
        public string StoreName { get; set; }

        [HmacOrder(2)]
        [JsonPropertyName("product")]
        public string ProductName { get; set; }

        [HmacOrder(3)]
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [HmacOrder(4)]
        [JsonPropertyName("owner")]
        public string Owner { get; set; }

        [HmacOrder(6)]
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
