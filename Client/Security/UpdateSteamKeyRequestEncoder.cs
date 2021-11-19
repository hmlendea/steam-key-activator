using NuciSecurity.HMAC;

using SteamKeyActivator.Client.Models;

namespace SteamKeyActivator.Client.Security
{
    public sealed class UpdateProductKeyRequestEncoder : HmacEncoder<UpdateProductKeyRequest>
    {
        public override string GenerateToken(UpdateProductKeyRequest obj, string sharedSecretKey)
        {
            string stringForSigning =
                obj.StoreName +
                obj.ProductName +
                obj.Key +
                obj.Owner +
                obj.Status;

            string hmacToken = ComputeHmacToken(stringForSigning, sharedSecretKey);

            return hmacToken;
        }
    }
}
