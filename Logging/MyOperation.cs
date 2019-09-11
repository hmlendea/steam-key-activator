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

        public static Operation KeyRetrieval => new MyOperation(nameof(KeyRetrieval));

        public static Operation KeyActivation => new MyOperation(nameof(KeyActivation));

        public static Operation KeyUpdate => new MyOperation(nameof(KeyUpdate));
    }
}