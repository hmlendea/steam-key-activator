using System.Text.Json.Serialization;
using NuciSecurity.HMAC;

namespace SteamKeyActivator.Client.Models
{
    public sealed class ProductKeyObject
    {
        [HmacOrder(1)]
        [JsonPropertyName("store")]
        public string Store { get; set; }

        [HmacOrder(2)]
        [JsonPropertyName("product")]
        public string Product { get; set; }

        [HmacOrder(3)]
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [HmacOrder(6)]
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
