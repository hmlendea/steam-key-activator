using NuciLog.Core;

namespace SteamKeyActivator.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name)
            : base(name)
        {
            
        }

        public static Operation SteamLogIn => new MyOperation(nameof(SteamLogIn));
    }
}