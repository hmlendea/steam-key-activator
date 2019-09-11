using NuciLog.Core;

namespace SteamKeyActivator.Logging
{
    public sealed class MyLogInfoKey : LogInfoKey
    {
        MyLogInfoKey(string name)
            : base(name)
        {
            
        }

        public static LogInfoKey Username => new MyLogInfoKey(nameof(Username));
    }
}