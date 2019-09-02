using NuciSecurity.HMAC;

using SteamKeyActivator.Client.Models;

namespace SteamKeyActivator.Client.Security
{
    public sealed class ProductKeyResponseEncoder : HmacEncoder<ProductKeyResponse>
    {
        public override string GenerateToken(ProductKeyResponse obj, string sharedSecretKey)
        {
            string stringForSigning = string.Empty;

            foreach (ProductKeyObject productKey in obj.ProductKeys)
            {
                if (productKey is null)
                {
                    continue;
                }
                
                stringForSigning +=
                    productKey.Store +
                    productKey.Product +
                    productKey.Key +
                    productKey.Status;
            }

            string hmacToken = ComputeHmacToken(stringForSigning, sharedSecretKey);

            return hmacToken;
        }
    }
}
