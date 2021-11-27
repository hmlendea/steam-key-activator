using NuciWeb.Steam.Models;

namespace SteamKeyActivator.Configuration
{
    public sealed class BotSettings
    {
        public int PageLoadTimeout { get; set; }

        public SteamAccount SteamAccount { get; set; }
    }
}
