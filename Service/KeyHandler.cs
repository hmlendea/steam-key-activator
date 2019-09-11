using SteamKeyActivator.Client;
using SteamKeyActivator.Configuration;

namespace SteamKeyActivator.Service
{
    public sealed class KeyUpdater : IKeyHandler
    {
        readonly IProductKeyManagerClient productKeyManagerClient;
        readonly BotSettings botSettings;

        public KeyUpdater(
            IProductKeyManagerClient productKeyManagerClient,
            BotSettings botSettings)
        {
            this.productKeyManagerClient = productKeyManagerClient;
            this.botSettings = botSettings;
        }

        public string GetRandomKey()
        {
            return productKeyManagerClient.GetProductKey("Unknown").Result;
        }

        public void MarkKeyAsInvalid(string key)
        {
            this.productKeyManagerClient.UpdateProductKey(key, "N/A", "Invalid", "N/A");
        }

        public void MarkKeyAsUsedBySomeoneElse(string key)
        {
            this.productKeyManagerClient.UpdateProductKey(key, "Unknown", "Used", "Unknown");
        }

        public void MarkKeyAsAlreadyOwned(string key)
        {
            this.productKeyManagerClient.UpdateProductKey(key, "Unknown", "AlreadyOwned", botSettings.SteamUsername);
        }

        public void MarkKeyAsActivated(string key, string productName)
        {
            this.productKeyManagerClient.UpdateProductKey(key, productName, "AlreadyOwned", botSettings.SteamUsername);
        }
    }
}
