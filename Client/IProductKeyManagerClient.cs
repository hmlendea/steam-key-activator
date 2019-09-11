using System.Threading.Tasks;

namespace SteamKeyActivator.Client
{
    public interface IProductKeyManagerClient
    {
        Task<string> GetProductKey(string status);

        Task UpdateProductKey(string key, string productName, string status);

        Task UpdateProductKey(string key, string productName, string status, string owner);
    }
}
