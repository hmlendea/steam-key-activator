using System.Text.Json.Serialization;
using NuciAPI.Requests;
using NuciSecurity.HMAC;

namespace SteamKeyActivator.Client.Models
{
    public class GetProductKeyRequest : Request
    {
        [HmacOrder(1)]
        [JsonPropertyName("store")]
        public string StoreName { get; set; }

        [HmacOrder(5)]
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
