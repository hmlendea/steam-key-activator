using System.IO;

namespace SteamKeyActivator.Configuration
{
    public sealed class CacheSettings
    {
        public string CacheDirectoryPath { get; set; }

        public string CookiesFilePath => Path.Combine(CacheDirectoryPath, "cookies.txt");

        public string LastSteamGuardCodeFilePath => Path.Combine(CacheDirectoryPath, "last-steamguard-code.txt");
    }
}
