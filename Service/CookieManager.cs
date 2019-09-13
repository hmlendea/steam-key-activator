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
    public sealed class CookieManager : ICookieManager
    {
        static string HomePageUrl => "https://store.steampowered.com";

        readonly IWebProcessor webProcessor;
        readonly IWebDriver webDriver;
        readonly CacheSettings cacheSettings;
        readonly ILogger logger;

        public CookieManager(
            IWebProcessor webProcessor,
            IWebDriver webDriver,
            CacheSettings cacheSettings,
            ILogger logger)
        {
            this.webProcessor = webProcessor;
            this.webDriver = webDriver;
            this.cacheSettings = cacheSettings;
            this.logger = logger;
        }

        public void LoadCookies()
        {
            logger.Info(MyOperation.CookieLoading, OperationStatus.Started);

            string cookiesFilePath = Path.Combine(cacheSettings.CacheDirectoryPath, "cookies.txt");

            if (!File.Exists(cookiesFilePath))
            {
                logger.Warn(
                    MyOperation.CookieLoading,
                    OperationStatus.Failure,
                    "The cookies file is missing");
                
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

        public void SaveCookies()
        {
            logger.Info(MyOperation.CookieSaving, OperationStatus.Started);

            string cookiesFilePath = Path.Combine(cacheSettings.CacheDirectoryPath, "cookies.txt"); 
            string cookiesFileContent = string.Empty;       
            ReadOnlyCollection<Cookie> cookies = webDriver.Manage().Cookies.AllCookies;

            if (cookies.Count == 0)
            {
                logger.Warn(
                    MyOperation.CookieSaving,
                    OperationStatus.Failure,
                    "There are no cookies to save");
                
                return;
            }

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
    }
}
