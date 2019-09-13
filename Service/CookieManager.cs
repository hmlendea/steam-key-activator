using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Web;

using NuciExtensions;
using NuciLog.Core;

using SteamKeyActivator.Configuration;
using SteamKeyActivator.Logging;

namespace SteamKeyActivator.Service
{
    public sealed class CookieManager : ICookieManager
    {
        const string HomePageUrl = "https://store.steampowered.com";

        readonly CacheSettings cacheSettings;
        readonly ILogger logger;

        public CookieManager(
            CacheSettings cacheSettings,
            ILogger logger)
        {
            this.cacheSettings = cacheSettings;
            this.logger = logger;
        }

        public CookieCollection LoadCookies()
        {
            logger.Info(MyOperation.CookieLoading, OperationStatus.Started);

            CookieCollection cookies = new CookieCollection();
            string cookiesFilePath = Path.Combine(cacheSettings.CookiesFilePath);

            if (!File.Exists(cookiesFilePath))
            {
                logger.Warn(
                    MyOperation.CookieLoading,
                    OperationStatus.Failure,
                    "The cookies file is missing");
                
                return cookies;
            }

            IEnumerable<string> cookiesFileLines = File.ReadAllLines(cookiesFilePath);

            foreach (string cookieLine in cookiesFileLines)
            {
                if (cookieLine.StartsWith('#'))
                {
                    continue;
                }

                string[] cookieLineFields = cookieLine.Split('\t');

                Cookie cookie = new Cookie();
                cookie.Domain = cookieLineFields[0];
                cookie.HttpOnly = bool.Parse(cookieLineFields[1].ToLower());
                cookie.Path = cookieLineFields[2];
                cookie.Secure = bool.Parse(cookieLineFields[3].ToLower());
                cookie.Name = cookieLineFields[5];
                cookie.Value = HttpUtility.UrlEncode(cookieLineFields[6]);

                if (cookieLineFields[4] != "0")
                {
                    cookie.Expires = DateTimeExtensions.FromUnixTime(cookieLineFields[4]);
                }

                cookies.Add(cookie);
            }

            logger.Debug(MyOperation.CookieLoading, OperationStatus.Success);

            return cookies;
        }

        public void SaveCookies(CookieCollection cookies)
        {
            logger.Info(MyOperation.CookieSaving, OperationStatus.Started);

            string cookiesFilePath = Path.Combine(cacheSettings.CookiesFilePath); 
            string cookiesFileContent = string.Empty;       

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

                if (cookie.Expires >= DateTime.Now)
                {
                    expiryTime = DateTimeExtensions
                        .GetElapsedUnixTime(cookie.Expires)
                        .TotalSeconds
                        .ToString();
                }

                cookiesFileContent +=
                    cookie.Domain + '\t' +
                    cookie.HttpOnly.ToString().ToUpper() + '\t' +
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
    }
}
