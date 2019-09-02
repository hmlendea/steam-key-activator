using System.Collections.Generic;

namespace SteamKeyActivator.Client.Models
{
    public sealed class ProductKeyResponse : SuccessResponse
    {
        public IEnumerable<ProductKeyObject> ProductKeys { get; set; }

        public ProductKeyResponse(ProductKeyObject productKey)
        {
            ProductKeys = new List<ProductKeyObject> { productKey };
        }

        public ProductKeyResponse(IEnumerable<ProductKeyObject> productKeys)
        {
            ProductKeys = productKeys;
        }
    }
}
