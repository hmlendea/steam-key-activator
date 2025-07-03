using System;
using NuciLog.Core;

using SteamKeyActivator.Client;
using SteamKeyActivator.Configuration;
using SteamKeyActivator.Logging;

namespace SteamKeyActivator.Service
{
    public sealed class KeyUpdater(
        IProductKeyManagerClient productKeyManagerClient,
        BotSettings botSettings,
        ILogger logger) : IKeyHandler
    {
        const string InvalidKeyStatus = "Invalid";
        const string UsedKeyStatus = "Used";
        const string AlreadyOwnedKeyStatus = "AlreadyOwned";
        const string RequiresBasedProductKeyStatus = "RequiresBaseProduct";
        const string RegionLockedStatus = "RegionLocked";

        const string UnknownProductName = "Unknown";
        const string UnknownProductOwner = "Unknown";

        public string GetRandomKey()
        {
            logger.Info(
                MyOperation.KeyRetrieval,
                OperationStatus.Started);

            string key;

            try
            {
                key = productKeyManagerClient.GetProductKey("Unknown").Result;
            }
            catch (Exception ex)
            {
                logger.Error(
                    MyOperation.KeyRetrieval,
                    OperationStatus.Failure,
                    ex);

                throw;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                logger.Error(
                    MyOperation.KeyRetrieval,
                    OperationStatus.Failure,
                    "The key is null or empty.");

                return null;
            }

            logger.Debug(
                MyOperation.KeyRetrieval,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.KeyCode, key));

            return key;
        }

        public void MarkKeyAsInvalid(string key)
        {
            logger.Info(
                MyOperation.KeyUpdate,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.KeyCode, key),
                new LogInfo(MyLogInfoKey.KeyStatus, InvalidKeyStatus));

            productKeyManagerClient.UpdateProductKey(
                key,
                productName: null,
                InvalidKeyStatus,
                owner: null);

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

            productKeyManagerClient.UpdateProductKey(
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

            productKeyManagerClient.UpdateProductKey(
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

            productKeyManagerClient.UpdateProductKey(
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

            productKeyManagerClient.UpdateProductKey(
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

            productKeyManagerClient.UpdateProductKey(
                key,
                productName,
                UsedKeyStatus,
                botSettings.SteamAccount.Username);

            logger.Debug(
                MyOperation.KeyUpdate,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.KeyCode, key));
        }
    }
}
