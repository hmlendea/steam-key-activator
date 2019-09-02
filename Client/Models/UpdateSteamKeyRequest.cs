namespace SteamKeyActivator.Client.Models
{
    public sealed class UpdateProductKeyRequest : Request
    {
        public string StoreName => "Steam";

        public string ProductName { get; set; }

        public string Key { get; set; }

        public string Status { get; set; }
    }
}
