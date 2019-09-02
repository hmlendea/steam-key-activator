using NuciSecurity.HMAC;

using SteamKeyActivator.Client.Models;

namespace SteamKeyActivator.Client.Security
{
    public sealed class GetProductKeyRequestEncoder : HmacEncoder<GetProductKeyRequest>
    {
        public override string GenerateToken(GetProductKeyRequest obj, string sharedSecretKey)
        {
            string stringForSigning =
                obj.StoreName +
                obj.ProductName +
                obj.Status;

            string hmacToken = ComputeHmacToken(stringForSigning, sharedSecretKey);

            return hmacToken;
        }
    }
}
