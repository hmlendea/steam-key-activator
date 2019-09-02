using System.Threading.Tasks;

namespace SteamKeyActivator.Client
{
    public interface IProductKeyManagerClient
    {
        Task<string> GetProductKey();

        Task UpdateProductKey(string key, string productName, string status);
    }
}
