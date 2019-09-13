using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Web;

using NuciExtensions;
using NuciLog.Core;
using NuciWeb;

using OpenQA.Selenium;

using SteamKeyActivator.Configuration;
using SteamKeyActivator.Logging;

namespace SteamKeyActivator.Service
{
    public sealed class CookieManager : ICookieManager
    {
        const string HomePageUrl = "https://store.steampowered.com";

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

            string cookiesFilePath = Path.Combine(cacheSettings.CookiesFilePath);

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

            IEnumerable<Cookie> cookies = ReadCookiesFromFile(cookiesFilePath);
            webDriver.Manage().Cookies.DeleteAllCookies();
            
            foreach(Cookie cookie in cookies)
            {
                webDriver.Manage().Cookies.AddCookie(cookie);
            }

            logger.Debug(MyOperation.CookieLoading, OperationStatus.Success);
        }

        public void SaveCookies()
        {
            logger.Info(MyOperation.CookieSaving, OperationStatus.Started);

            string cookiesFilePath = Path.Combine(cacheSettings.CookiesFilePath); 
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
                string expiryTime = "0";

                if (!(cookie.Expiry is null))
                {
                    expiryTime = DateTimeExtensions
                        .GetElapsedUnixTime(cookie.Expiry.Value)
                        .TotalSeconds
                        .ToString();
                }

                cookiesFileContent +=
                    cookie.Domain + '\t' +
                    cookie.IsHttpOnly.ToString().ToUpper() + '\t' +
                    cookie.Path + '\t' +
                    cookie.Secure.ToString().ToUpper() + '\t' +
                    expiryTime + '\t' +
                    cookie.Name + '\t' +
                    HttpUtility.UrlDecode(cookie.Value) +
                    Environment.NewLine;
            }

            File.WriteAllText(cookiesFilePath, cookiesFileContent);

            logger.Debug(MyOperation.CookieSaving, OperationStatus.Success);
        }

        IEnumerable<Cookie> ReadCookiesFromFile(string cookiesFilePath)
        {
            IEnumerable<string> cookiesFileLines = File.ReadAllLines(cookiesFilePath);
            IList<Cookie> cookies = new List<Cookie>();

            foreach (string cookieLine in cookiesFileLines)
            {
                if (cookieLine.StartsWith("#"))
                {
                    continue;
                }

                string[] cookieLineFields = cookieLine.Split('\t');

                string cookieDomain = cookieLineFields[0];
                string cookiePath = cookieLineFields[2];
                string cookieName = cookieLineFields[5];
                string cookieValue = HttpUtility.UrlEncode(cookieLineFields[6]);
                DateTime? cookieExpiry = null;

                if (cookieLineFields[4] != "0")
                {
                    cookieExpiry = DateTimeExtensions.FromUnixTime(cookieLineFields[4]);
                }

                Cookie cookie = new Cookie(cookieName, cookieValue, cookieDomain, cookiePath, cookieExpiry);
                cookies.Add(cookie);
            }

            return cookies;
        }
    }
}
