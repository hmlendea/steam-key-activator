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
                obj.Key;

            if (!(obj.Owner is null))
            {
                stringForSigning += obj.Owner;
            }

            stringForSigning += obj.Status;

            return ComputeHmacToken(stringForSigning, sharedSecretKey);
        }
    }
}
