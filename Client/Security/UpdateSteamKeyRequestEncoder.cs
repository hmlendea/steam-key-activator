using NuciSecurity.HMAC;

using SteamKeyActivator.Client.Models;

namespace SteamKeyActivator.Client.Security
{
    public sealed class UpdateProductKeyRequestEncoder : HmacEncoder<UpdateProductKeyRequest>
    {
        public override string GenerateToken(UpdateProductKeyRequest obj, string sharedSecretKey)
        {
            string stringForSigning = obj.StoreName;

            if (!(obj.ProductName is null))
            {
                stringForSigning += obj.ProductName;
            }

            stringForSigning += obj.Key;

            if (!(obj.Owner is null))
            {
                stringForSigning += obj.Owner;
            }

            stringForSigning += obj.Status;

            return ComputeHmacToken(stringForSigning, sharedSecretKey);
        }
    }
}
