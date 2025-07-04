namespace SteamKeyActivator.Service
{
    public interface IKeyHandler
    {
        string GetRandomKey();

        void MarkKeyAsInvalid(string key);

        void MarkKeyAsUsedBySomeoneElse(string key);

        void MarkKeyAsAlreadyOwned(string key);

        void MarkKeyAsRequiresBaseProduct(string key);

        void MarkKeyAsRegionLocked(string key);

        void MarkKeyAsActivated(string key, string productName);
    }
}
