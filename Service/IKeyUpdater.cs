namespace SteamKeyActivator.Service
{
    public interface IKeyUpdater
    {
        void MarkKeyAsInvalid(string key);

        void MarkKeyAsUsedBySomeoneElse(string key);

        void MarkKeyAsAlreadyOwned(string key);

        void MarkKeyAsActivated(string key);
    }
}
