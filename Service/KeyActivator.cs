using System;

using NuciLog.Core;
using NuciWeb.Steam;

using SteamKeyActivator.Configuration;
using SteamKeyActivator.Logging;

namespace SteamKeyActivator.Service
{
    public sealed class KeyActivator(
        ISteamProcessor steamProcessor,
        IKeyHandler keyHandler,
        BotSettings botSettings,
        ILogger logger) : IKeyActivator
    {
        const string KeyActivationUrl = "https://store.steampowered.com/account/registerkey";

        public void ActivateRandomPkmKey()
        {
            steamProcessor.LogIn(botSettings.SteamAccount);
            string key = keyHandler.GetRandomKey();

            ActivateKey(key);
        }

        void ActivateKey(string key)
        {
            logger.Info(
                MyOperation.KeyActivation,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.KeyCode, key));

            try
            {
                string productName = steamProcessor.ActivateKey(key);
                keyHandler.MarkKeyAsActivated(key, productName);

                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Success,
                    new LogInfo(MyLogInfoKey.KeyCode, key));
            }
            catch (KeyActivationException ex)
            {
                HandleActivationError(key, ex);
                return;
            }
        }

        void HandleActivationError(string key, KeyActivationException ex)
        {
            if (ex.Code == KeyActivationErrorCode.InvalidProductKey)
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "Invalid product key",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                keyHandler.MarkKeyAsInvalid(key);
                return;
            }

            if (ex.Code == KeyActivationErrorCode.AlreadyActivatedDifferentAccount)
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "Key already activated by a different account",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                keyHandler.MarkKeyAsUsedBySomeoneElse(key);
                return;
            }

            if (ex.Code == KeyActivationErrorCode.AlreadyActivatedCurrentAccount)
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "Product already owned by this account",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                keyHandler.MarkKeyAsAlreadyOwned(key);
                return;
            }

            if (ex.Code == KeyActivationErrorCode.BaseProductRequired)
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "A base product is required in order to activate this key",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                keyHandler.MarkKeyAsRequiresBaseProduct(key);
                return;
            }

            if (ex.Code == KeyActivationErrorCode.RegionLocked)
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "The key is locked to a specific region",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                keyHandler.MarkKeyAsRegionLocked(key);
                return;
            }

            if (ex.Code == KeyActivationErrorCode.TooManyAttempts)
            {
                logger.Warn(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "Key activation limit reached",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                return;
            }

            if (ex.Code == KeyActivationErrorCode.Unexpected)
            {
                logger.Warn(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "An unexpected error has occurred",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                return;
            }

            throw new FormatException($"Unrecognised error: \"{ex.Message}\"");
        }
    }
}
