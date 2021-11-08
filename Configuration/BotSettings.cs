namespace SteamKeyActivator.Configuration
{
    public sealed class BotSettings
    {
        public int PageLoadTimeout { get; set; }

        public string SteamUsername { get; set; }

        public string SteamPassword { get; set; }

        public string SteamGuardTotpKey { get; set; }
    }
}
