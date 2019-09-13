using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Authentication;

using NuciLog.Core;
using NuciWeb;

using OpenQA.Selenium;

using SteamKeyActivator.Configuration;
using SteamKeyActivator.Logging;

namespace SteamKeyActivator.Service
{
    public sealed class KeyActivator : IKeyActivator
    {
        static string HomePageUrl => "https://store.steampowered.com";
        public string LoginUrl => $"{HomePageUrl}/login/?redir=&redir_ssl=1";
        public string KeyActivationUrl => $"{HomePageUrl}/account/registerkey";

        readonly IWebProcessor webProcessor;
        readonly IWebDriver webDriver;
        readonly ISteamAuthenticator steamAuthenticator;
        readonly IKeyHandler keyHandler;
        readonly BotSettings botSettings;
        readonly CacheSettings cacheSettings;
        readonly ILogger logger;

        public KeyActivator(
            IWebProcessor webProcessor,
            IWebDriver webDriver,
            ISteamAuthenticator steamAuthenticator,
            IKeyHandler keyHandler,
            BotSettings botSettings,
            CacheSettings cacheSettings,
            ILogger logger)
        {
            this.webProcessor = webProcessor;
            this.webDriver = webDriver;
            this.steamAuthenticator = steamAuthenticator;
            this.keyHandler = keyHandler;
            this.botSettings = botSettings;
            this.cacheSettings = cacheSettings;
            this.logger = logger;
        }

        public void ActivateRandomPkmKey()
        {
            LoadCookies();
            
            steamAuthenticator.LogIn();
            string key = keyHandler.GetRandomKey();

            ActivateKey(key);
            SaveCookies();
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

            if (errorMessage.Contains("too many recent activation attempts") ||
                errorMessage.Contains("prea multe încercări de activare recente"))
            {
                logger.Debug(
                    MyOperation.KeyActivation,
                    OperationStatus.Failure,
                    "Key activation limit reached",
                    new LogInfo(MyLogInfoKey.KeyCode, key));
                return;
            }

            throw new FormatException($"Unrecognised error message: \"{errorMessage}\"");
        }

        void SaveCookies()
        {
            logger.Info(MyOperation.CookieSaving, OperationStatus.Started);

            string cookiesFilePath = Path.Combine(cacheSettings.CacheDirectoryPath, "cookies.txt"); 
            string cookiesFileContent = string.Empty;       
            ReadOnlyCollection<Cookie> cookies = webDriver.Manage().Cookies.AllCookies;

            foreach (Cookie cookie in cookies)
            {
                cookiesFileContent +=
                    cookie.Name + "Î" +
                    cookie.Value + "Î" +
                    cookie.Domain + "Î" +
                    cookie.Path + "Î" +
                    cookie.Expiry.ToString() +
                    Environment.NewLine;
            }

            File.WriteAllText(cookiesFilePath, cookiesFileContent);

            logger.Debug(MyOperation.CookieSaving, OperationStatus.Success);
        }

        void LoadCookies()
        {
            logger.Info(MyOperation.CookieLoading, OperationStatus.Started);

            string cookiesFilePath = Path.Combine(cacheSettings.CacheDirectoryPath, "cookies.txt");

            if (!File.Exists(cookiesFilePath))
            {
                return;
            }

            By logoSelector = By.Id("logo_holder");

            webProcessor.GoToUrl(HomePageUrl);
            webProcessor.WaitForElementToBeVisible(logoSelector);

            IEnumerable<string> cookiesFileLines = File.ReadAllLines(cookiesFilePath);

            webDriver.Manage().Cookies.DeleteAllCookies();

            foreach (string cookieLine in cookiesFileLines)
            {
                string[] cookieLineFields = cookieLine.Split('Î');
                DateTime? expiry = null;

                if (!string.IsNullOrWhiteSpace(cookieLineFields[4]))
                {
                    expiry = DateTime.Parse(cookieLineFields[4]);
                }

                Cookie cookie = new Cookie(
                    cookieLineFields[0],
                    cookieLineFields[1],
                    cookieLineFields[2],
                    cookieLineFields[3],
                    expiry);

                webDriver.Manage().Cookies.AddCookie(cookie);
            }

            logger.Debug(MyOperation.CookieLoading, OperationStatus.Success);
        }
    }
}
