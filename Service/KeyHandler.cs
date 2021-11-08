using NuciLog.Core;

using SteamKeyActivator.Client;
using SteamKeyActivator.Configuration;
using SteamKeyActivator.Logging;

namespace SteamKeyActivator.Service
{
    public sealed class KeyUpdater : IKeyHandler
    {
        const string InvalidKeyStatus = "Invalid";
        const string UsedKeyStatus = "Used";
        const string AlreadyOwnedKeyStatus = "AlreadyOwned";
        const string RequiresBasedProductKeyStatus = "RequiresBaseProduct";
        const string RegionLockedStatus = "RegionLocked";

        const string InvalidProductName = "N/A";
        const string UnknownProductName = "Unknown";
        const string InvalidProductOwner = "N/A";
        const string UnknownProductOwner = "Unknown";

        readonly IProductKeyManagerClient productKeyManagerClient;
        readonly BotSettings botSettings;
        readonly ILogger logger;

        public KeyUpdater(
            IProductKeyManagerClient productKeyManagerClient,
            BotSettings botSettings,
            ILogger logger)
        {
            this.productKeyManagerClient = productKeyManagerClient;
            this.botSettings = botSettings;
            this.logger = logger;
        }

        public string GetRandomKey()
        {
            logger.Info(
                MyOperation.KeyRetrieval,
                OperationStatus.Started);

            string key = productKeyManagerClient.GetProductKey("Unknown")?.Result;

            if (string.IsNullOrWhiteSpace(key))
            {
                logger.Error(
                    MyOperation.KeyRetrieval,
                    OperationStatus.Failure);
            }
            else
            {
                logger.Debug(
                    MyOperation.KeyRetrieval,
                    OperationStatus.Success,
                    new LogInfo(MyLogInfoKey.KeyCode, key));
            }

            return key;
        }

        public void MarkKeyAsInvalid(string key)
        {
            logger.Info(
                MyOperation.KeyUpdate,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.KeyCode, key),
                new LogInfo(MyLogInfoKey.KeyStatus, InvalidKeyStatus));

            this.productKeyManagerClient.UpdateProductKey(
                key,
                InvalidProductName,
                InvalidKeyStatus,
                InvalidProductOwner);

            logger.Debug(
                MyOperation.KeyUpdate,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.KeyCode, key));
        }

        public void MarkKeyAsUsedBySomeoneElse(string key)
        {
            logger.Info(
                MyOperation.KeyUpdate,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.KeyCode, key),
                new LogInfo(MyLogInfoKey.KeyStatus, UsedKeyStatus));

            this.productKeyManagerClient.UpdateProductKey(
                key,
                UnknownProductName,
                UsedKeyStatus,
                UnknownProductOwner);

            logger.Debug(
                MyOperation.KeyUpdate,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.KeyCode, key));
        }

        public void MarkKeyAsAlreadyOwned(string key)
        {
            logger.Info(
                MyOperation.KeyUpdate,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.KeyCode, key),
                new LogInfo(MyLogInfoKey.KeyStatus, AlreadyOwnedKeyStatus));

            this.productKeyManagerClient.UpdateProductKey(
                key,
                UnknownProductName,
                AlreadyOwnedKeyStatus,
                UnknownProductOwner);

            logger.Debug(
                MyOperation.KeyUpdate,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.KeyCode, key));
        }

        public void MarkKeyAsRequiresBaseProduct(string key)
        {
            logger.Info(
                MyOperation.KeyUpdate,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.KeyCode, key),
                new LogInfo(MyLogInfoKey.KeyStatus, RequiresBasedProductKeyStatus));

            this.productKeyManagerClient.UpdateProductKey(
                key,
                UnknownProductName,
                RequiresBasedProductKeyStatus,
                UnknownProductOwner);

            logger.Debug(
                MyOperation.KeyUpdate,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.KeyCode, key));
        }

        public void MarkKeyAsRegionLocked(string key)
        {
            logger.Info(
                MyOperation.KeyUpdate,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.KeyCode, key),
                new LogInfo(MyLogInfoKey.KeyStatus, RegionLockedStatus));

            this.productKeyManagerClient.UpdateProductKey(
                key,
                UnknownProductName,
                RegionLockedStatus);

            logger.Debug(
                MyOperation.KeyUpdate,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.KeyCode, key));
        }

        public void MarkKeyAsActivated(string key, string productName)
        {
            logger.Info(
                MyOperation.KeyUpdate,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.KeyCode, key),
                new LogInfo(MyLogInfoKey.KeyStatus, UsedKeyStatus));

            this.productKeyManagerClient.UpdateProductKey(
                key,
                productName,
                UsedKeyStatus,
                botSettings.SteamUsername);

            logger.Debug(
                MyOperation.KeyUpdate,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.KeyCode, key));
        }
    }
}
