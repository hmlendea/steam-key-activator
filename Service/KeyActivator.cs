using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

using SteamKeyActivator.Configuration;

namespace SteamKeyActivator.Service
{
    public sealed class KeyActivator : IKeyActivator
    {
        const string KeyActivationEndpoint = "https://store.steampowered.com/account/ajaxregisterkey";

        readonly ICookieManager cookieManager;
        readonly HttpSettings httpSettings;

        readonly HttpClientHandler httpClientHandler;
        readonly HttpClient httpClient;

        public KeyActivator(
            ICookieManager cookieManager,
            HttpSettings httpSettings)
        {
            this.cookieManager = cookieManager;
            this.httpSettings = httpSettings;

            httpClientHandler = new HttpClientHandler();

            CookieCollection cookies = cookieManager.LoadCookies();
            if (!(cookies is null))
            {
                httpClientHandler.CookieContainer = new CookieContainer();
                httpClientHandler.CookieContainer.Add(cookies);
            }

            httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                httpSettings.UserAgent);
        }

        public void ActivateRandomPkmKey()
        {
            Uri uri = new Uri(KeyActivationEndpoint);
            HttpRequestMessage request = BuildRequest("...");
            HttpResponseMessage response = httpClient.SendAsync(request).Result; // TODO: Broken async

            cookieManager.SaveCookies(httpClientHandler.CookieContainer.GetCookies(uri));
        }

        HttpRequestMessage BuildRequest(string key)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(KeyActivationEndpoint);

            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("DNT", "1");
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "en-GB,en;q=0.9,ro;q=0.8");
            request.Headers.Add("X-Prototype-Version", "1.7");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            //request.Headers.Add("Cookie", "...");
            request.Headers.Add("Connection", "keep-alive");
            //request.Headers.Add("Content-type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.Headers.Add("Accept", "text/javascript, text/html, application/xml, text/xml, */*");
            request.Headers.Add("Referer", "https://store.steampowered.com/account/registerkey");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Origin", "https://store.steampowered.com");

            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("product_key", key),
                new KeyValuePair<string, string>("sessionid", "..."),
            });

            return request;
        }
    }
}