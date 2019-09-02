namespace SteamKeyActivator.Client.Models
{
    public class GetProductKeyRequest : Request
    {
        public string StoreName { get; set; }

        public string ProductName { get; set; }

        public string Status { get; set; }
    }
}
