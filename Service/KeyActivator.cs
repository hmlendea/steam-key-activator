using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

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

        readonly IWebProcessor webProcessor;
        readonly IWebDriver webDriver;
        readonly BotSettings botSettings;
        readonly CacheSettings cacheSettings;
        readonly ILogger logger;

        public KeyActivator(
            IWebProcessor webProcessor,
            IWebDriver webDriver,
            BotSettings botSettings,
            CacheSettings cacheSettings,
            ILogger logger)
        {
            this.webProcessor = webProcessor;
            this.webDriver = webDriver;
            this.botSettings = botSettings;
            this.cacheSettings = cacheSettings;
            this.logger = logger;
        }

        public void ActivateRandomPkmKey()
        {
            LoadCookies();

            bool isLogInRequired = CheckIfLogInIsRequired();
            
            if (isLogInRequired)
            {
                LogIn();
            }
        }

        bool CheckIfLogInIsRequired()
        {
            webProcessor.GoToUrl(HomePageUrl);

            By logoSelector = By.Id("logo_holder");
            By avatarSelector = By.XPath("//a[contains(@class,'user_avatar')]");

            webProcessor.WaitForElementToBeVisible(logoSelector);

            if (webProcessor.IsElementVisible(avatarSelector))
            {
                return false;
            }

            return true;
        }

        void LogIn()
        {
            logger.Info(
                MyOperation.SteamLogIn,
                OperationStatus.Started,
                new LogInfo(MyLogInfoKey.Username, botSettings.SteamUsername));

            webProcessor.GoToUrl(LoginUrl);

            By usernameInputSelector = By.Id("input_username");
            By passwordInputSelector = By.Id("input_password");
            By steamGuardCodeInputSelector = By.Id("twofactorcode_entry");
            By steamGuardSubmitButtonSelector = By.XPath("//*[@id='login_twofactorauth_buttonset_entercode']/div[1]");
            By avatarSelector = By.XPath("//a[contains(@class,'user_avatar')]");

            webProcessor.SetText(usernameInputSelector, botSettings.SteamUsername);
            webProcessor.SetText(passwordInputSelector, botSettings.SteamPassword);
            
            webProcessor.Click(By.XPath(@"//*[@id='login_btn_signin']/button"));
            webProcessor.WaitForAnyElementToBeVisible(steamGuardCodeInputSelector, avatarSelector);

            if (webProcessor.IsElementVisible(steamGuardCodeInputSelector))
            {
                webProcessor.SetText(steamGuardCodeInputSelector, botSettings.SteamGuardCode);
                webProcessor.Click(steamGuardSubmitButtonSelector);
            }

            webProcessor.WaitForElementToBeVisible(avatarSelector);

            SaveCookies();

            logger.Debug(
                MyOperation.SteamLogIn,
                OperationStatus.Success);
        }

        void SaveCookies()
        {
            webProcessor.GoToUrl(HomePageUrl);
            
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
        }

        void LoadCookies()
        {
            string cookiesFilePath = Path.Combine(cacheSettings.CacheDirectoryPath, "cookies.txt");

            if (!File.Exists(cookiesFilePath))
            {
                return;
            }

            webProcessor.GoToUrl(HomePageUrl);

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

            webProcessor.GoToUrl("about:blank");
        }
    }
}
