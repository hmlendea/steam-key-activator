using System;

using NuciLog.Core;
using NuciWeb;

using OpenQA.Selenium;

using SteamKeyActivator.Configuration;
using SteamKeyActivator.Logging;

namespace SteamKeyActivator.Service
{
    public sealed class KeyActivator : IKeyActivator
    {
        const string KeyActivationUrl = "https://store.steampowered.com/account/registerkey";

        readonly IWebProcessor webProcessor;
        readonly ISteamAuthenticator steamAuthenticator;
        readonly IKeyHandler keyHandler;
        readonly BotSettings botSettings;
        readonly ILogger logger;

        public KeyActivator(
            IWebProcessor webProcessor,
            ISteamAuthenticator steamAuthenticator,
            IKeyHandler keyHandler,
            BotSettings botSettings,
            ILogger logger)
        {
            this.webProcessor = webProcessor;
            this.steamAuthenticator = steamAuthenticator;
            this.keyHandler = keyHandler;
            this.botSettings = botSettings;
            this.logger = logger;
        }

        public void ActivateRandomPkmKey()
        {
            steamAuthenticator.LogIn();
            string key = keyHandler.GetRandomKey();

            ActivateKey(key);
        }

        void ActivateKey(string key)
        {
            logger.Info(
                MyOperation.KeyActivation,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.KeyCode, key));

            By keyInputSelector = By.Id("product_key");
            By keyActivationButtonSelector = By.Id("register_btn");
            By agreementCheckboxSelector = By.Id("accept_ssa");

            By errorSelector = By.Id("error_display");
            By receiptSelector = By.Id("receipt_form");

            By productNameSelector = By.ClassName("registerkey_lineitem");

            if (!webProcessor.IsElementVisible(keyInputSelector))
            {
                webProcessor.GoToUrl(KeyActivationUrl);
            }

            webProcessor.SetText(keyInputSelector, key);
            webProcessor.UpdateCheckbox(agreementCheckboxSelector, true);

            webProcessor.Click(keyActivationButtonSelector);

            webProcessor.WaitForAnyElementToBeVisible(errorSelector, receiptSelector);

            if (webProcessor.IsElementVisible(errorSelector))
            {
                string errorMessage = webProcessor.GetText(errorSelector);
                HandleActivationError(key, errorMessage);
                return;
            }
            
            string productName = webProcessor.GetText(productNameSelector);
            keyHandler.MarkKeyAsActivated(key, productName);

            logger.Debug(
                MyOperation.KeyActivation,
                OperationStatus.Success,
                new LogInfo(MyLogInfoKey.KeyCode, key));
        }

        void HandleActivationError(string key, string errorMessage)
        {
            if (errorMessage.Contains("is not valid") ||
                errorMessage.Contains("nu este valid"))
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "Invalid product key",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                keyHandler.MarkKeyAsInvalid(key);
                return;
            }

            if (errorMessage.Contains("activated by a different Steam account") ||
                errorMessage.Contains("activat de un cont Steam diferit"))
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "Key already activated by a different account",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                keyHandler.MarkKeyAsUsedBySomeoneElse(key);
                return;
            }

            if (errorMessage.Contains("This Steam account already owns the product") ||
                errorMessage.Contains("Contul acesta Steam deține deja produsul"))
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "Product already owned by this account",
                    new LogInfo(MyLogInfoKey.KeyCode, key));
                    
                keyHandler.MarkKeyAsAlreadyOwned(key);
                return;
            }

            if (errorMessage.Contains("requires ownership of another product") ||
                errorMessage.Contains("necesită deținerea unui alt produs"))
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "A base product is required in order to activate this key",
                    new LogInfo(MyLogInfoKey.KeyCode, key));
                    
                keyHandler.MarkKeyAsRequiresBaseProduct(key);
                return;
            }

            if (errorMessage.Contains("this product is not available for purchase in this country") ||
                errorMessage.Contains("acest produs nu este disponibil pentru achiziție în această țară"))
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "The key is locked to a specific region",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                keyHandler.MarkKeyAsRegionLocked(key);
                return;
            }

            if (errorMessage.Contains("too many recent activation attempts") ||
                errorMessage.Contains("prea multe încercări de activare recente"))
            {
                logger.Warn(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "Key activation limit reached",
                    new LogInfo(MyLogInfoKey.KeyCode, key));

                return;
            }

            if (errorMessage.Contains("An unexpected error has occurred") ||
                errorMessage.Contains("A apărut o eroare neașteptată"))
            {
                logger.Warn(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "An unexpected error has occurred",
                    new LogInfo(MyLogInfoKey.KeyCode, key));
                    
                return;
            }

            throw new FormatException($"Unrecognised error message: \"{errorMessage}\"");
        }
    }
}
