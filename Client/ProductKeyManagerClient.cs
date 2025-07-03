using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using NuciAPI.Responses;
using NuciSecurity.HMAC;

using SteamKeyActivator.Client.Models;
using SteamKeyActivator.Configuration;

namespace SteamKeyActivator.Client
{
    public sealed class ProductKeyManagerClient(ProductKeyManagerSettings settings) : IProductKeyManagerClient
    {
        readonly HttpClient httpClient = new();

        public async Task<string> GetProductKey(string status)
        {
            string endpoint = BuildGetRequestUrl(status);

            HttpResponseMessage httpResponse = await httpClient.GetAsync(endpoint);

            if (!httpResponse.IsSuccessStatusCode)
            {
                ErrorResponse errorResponse = await DeserialiseErrorResponse(httpResponse);
                throw new HttpRequestException(errorResponse.Message);
            }

            ProductKeyResponse response = await DeserialiseSuccessResponse<ProductKeyResponse>(httpResponse);
            return response.ProductKeys.First().Key;
        }

        public async Task UpdateProductKey(string key, string productName, string status)
            => await UpdateProductKey(key, productName, status, null);

        public async Task UpdateProductKey(string key, string productName, string status, string owner)
        {
            string endpoint = BuildUpdateRequestUrl(key, productName, status, owner);

            HttpResponseMessage httpResponse = await httpClient.PutAsync(endpoint, null);

            if (!httpResponse.IsSuccessStatusCode)
            {
                ErrorResponse errorResponse = await DeserialiseErrorResponse(httpResponse);
                throw new HttpRequestException(errorResponse.Message);
            }
        }

        static async Task<ErrorResponse> DeserialiseErrorResponse(HttpResponseMessage httpResponse)
        {
            string responseString = await httpResponse.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseString))
            {
                return new ErrorResponse($"Request failed with status code {(int)httpResponse.StatusCode} ({httpResponse.StatusCode})");
            }

            return JsonConvert.DeserializeObject<ErrorResponse>(responseString);
        }

        static async Task<TResponse> DeserialiseSuccessResponse<TResponse>(HttpResponseMessage httpResponse)
        {
            string responseString = await httpResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResponse>(responseString);
        }

        string BuildGetRequestUrl(string status)
        {
            GetProductKeyRequest request = new()
            {
                StoreName = "Steam",
                Status = status
            };
            request.HmacToken = HmacEncoder.GenerateToken(request, settings.SharedSecretKey);

            return BuildRequestUrl(request.StoreName, request.Status, request.HmacToken);
        }

        string BuildUpdateRequestUrl(string key, string productName, string status, string owner)
        {
            UpdateProductKeyRequest request = new()
            {
                StoreName = "Steam",
                ProductName = productName,
                Key = key,
                Owner = owner,
                Status = status
            };
            request.HmacToken = HmacEncoder.GenerateToken(request, settings.SharedSecretKey);

            return BuildRequestUrl(request.StoreName, request.ProductName, request.Key, request.Owner, request.Status, request.HmacToken);
        }

        string BuildRequestUrl(string storeName, string status, string hmacToken)
            => BuildRequestUrl(storeName, productName: null, key: null, owner: null, status, hmacToken);

        string BuildRequestUrl(string storeName, string productName, string key, string owner, string status, string hmacToken)
        {
            string endpoint = $"{settings.ApiUrl}?store={HttpUtility.UrlEncode(storeName)}";

            if (!string.IsNullOrWhiteSpace(productName))
            {
                endpoint += $"&product={productName}";
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                endpoint += $"&key={key}";
            }

            if (!string.IsNullOrWhiteSpace(owner))
            {
                endpoint += $"&owner={owner}";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                endpoint += $"&status={status}";
            }

            endpoint += $"&hmac={HttpUtility.UrlEncode(hmacToken)}";

            return endpoint;
        }
    }
}
