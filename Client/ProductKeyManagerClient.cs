using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;

using NuciSecurity.HMAC;

using SteamKeyActivator.Client.Models;
using SteamKeyActivator.Configuration;

namespace SteamKeyActivator.Client
{
    public sealed class ProductKeyManagerClient : IProductKeyManagerClient
    {
        readonly HttpClient httpClient;

        readonly IHmacEncoder<GetProductKeyRequest> getRequestEncoder;
        readonly IHmacEncoder<UpdateProductKeyRequest> updateRequestEncoder;
        readonly IHmacEncoder<ProductKeyResponse> getResponseEncoder;

        readonly ProductKeyManagerSettings settings;

        public ProductKeyManagerClient(
            IHmacEncoder<GetProductKeyRequest> getRequestEncoder,
            IHmacEncoder<UpdateProductKeyRequest> updateRequestEncoder,
            IHmacEncoder<ProductKeyResponse> getResponseEncoder,
            ProductKeyManagerSettings settings)
        {
            this.getRequestEncoder = getRequestEncoder;
            this.updateRequestEncoder = updateRequestEncoder;
            this.getResponseEncoder = getResponseEncoder;
            this.settings = settings;

            httpClient = new HttpClient();
        }

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

        async Task<ErrorResponse> DeserialiseErrorResponse(HttpResponseMessage httpResponse)
        {
            string responseString = await httpResponse.Content.ReadAsStringAsync();
            ErrorResponse response = null;

            if (!string.IsNullOrWhiteSpace(responseString))
            {
                response = JsonConvert.DeserializeObject<ErrorResponse>(responseString);
            }
            else
            {
                response = new ErrorResponse($"Request failed with status code {(int)httpResponse.StatusCode} ({httpResponse.StatusCode})");
            }

            return response;
        }

        async Task<TResponse> DeserialiseSuccessResponse<TResponse>(HttpResponseMessage httpResponse)
        {
            string responseString = await httpResponse.Content.ReadAsStringAsync();
            TResponse response = JsonConvert.DeserializeObject<TResponse>(responseString);

            return response;
        }

        string BuildGetRequestUrl(string status)
        {
            GetProductKeyRequest request = new GetProductKeyRequest();
            request.StoreName = "Steam";
            request.Status = status;
            request.HmacToken = getRequestEncoder.GenerateToken(request, settings.SharedSecretKey);

            string endpoint =
                $"{settings.ApiUrl}" +
                $"?store={request.StoreName}" +
                $"&status={request.Status}" +
                $"&hmac={request.HmacToken}";

            return BuildRequestUrl(request.StoreName, request.Status, request.HmacToken);
        }

        string BuildUpdateRequestUrl(string key, string productName, string status, string owner)
        {
            UpdateProductKeyRequest request = new UpdateProductKeyRequest();
            request.StoreName = "Steam";
            request.ProductName = productName;
            request.Key = key;
            request.Owner = owner;
            request.Status = status;
            request.HmacToken = updateRequestEncoder.GenerateToken(request, settings.SharedSecretKey);

            return BuildRequestUrl(request.StoreName, request.ProductName, request.Key, request.Owner, request.Status, request.HmacToken);
        }

        string BuildRequestUrl(string storeName, string status, string hmacToken)
            => BuildRequestUrl(storeName, null, null, status, null, hmacToken);

        string BuildRequestUrl(string storeName, string productName, string key, string status, string owner, string hmacToken)
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
