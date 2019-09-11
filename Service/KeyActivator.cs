using NuciWeb;

namespace SteamKeyActivator.Service
{
    public sealed class KeyActivator : IKeyActivator
    {
        static string HomePage => "https://store.steampowered.com/";

        readonly IWebProcessor webProcessor;

        public KeyActivator(IWebProcessor webProcessor)
        {
            this.webProcessor = webProcessor;
        }

        public void ActivateRandomPkmKey()
        {
            webProcessor.GoToUrl(HomePage);
        }
    }
}