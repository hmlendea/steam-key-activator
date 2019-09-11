using System.Collections.Generic;

namespace SteamKeyActivator.Client.Models
{
    public sealed class ProductKeyResponse : SuccessResponse
    {
        public IEnumerable<ProductKeyObject> ProductKeys { get; set; }
    }
}
