namespace SteamKeyActivator.Configuration
{
    public sealed class DebugSettings
    {
        public bool IsDebugMode { get; set; }

        public bool IsHeadless => !IsDebugMode;
    }
}
