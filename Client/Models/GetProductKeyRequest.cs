namespace SteamKeyActivator.Client.Models
{
    public class GetProductKeyRequest : Request
    {
        public string StoreName => "Steam";

        public string ProductName { get; set; }

        public string Status => "Unknown";
    }
}
