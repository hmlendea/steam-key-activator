using System.Net;

namespace SteamKeyActivator.Service
{
    public interface ICookieManager
    {
        CookieCollection LoadCookies();

        void SaveCookies(CookieCollection cookies);
    }
}
