using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using NuciAPI.Requests;
using NuciAPI.Responses;
using NuciExtensions;
using NuciSecurity.HMAC;

using SteamKeyActivator.Client.Models;
using SteamKeyActivator.Configuration;

namespace SteamKeyActivator.Client
{
    public sealed class ProductKeyManagerClient(
        ProductKeyManagerSettings settings) : IProductKeyManagerClient
    {
        readonly HttpClient httpClient = new();

        public async Task<string> GetProductKey(string status)
        {
            HttpResponseMessage httpResponse = await SendRequest(HttpMethod.Get, new GetProductKeyRequest()
            {
                StoreName = "Steam",
                Status = status
            });

            ProductKeyResponse response = await DeserialiseSuccessResponse<ProductKeyResponse>(httpResponse);

            return response.ProductKeys.First().Key;
        }

        public async Task UpdateProductKey(string key, string productName, string status)
            => await UpdateProductKey(key, productName, status, null);

        public async Task UpdateProductKey(string key, string productName, string status, string owner)
            => await SendRequest(HttpMethod.Put, new UpdateProductKeyRequest()
            {
                StoreName = "Steam",
                ProductName = productName,
                Key = key,
                Owner = owner,
                Status = status
            });

        private async Task<HttpResponseMessage> SendRequest<TRequest>(
            HttpMethod httpMethod,
            TRequest request) where TRequest : Request
        {
            request.HmacToken = HmacEncoder.GenerateToken(request, settings.SharedSecretKey);

            HttpRequestMessage httpRequest = new(httpMethod, $"{settings.ApiUrl}/ProductKeys")
            {
                Content = new StringContent(
                    request.ToJson(),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);

            if (!httpResponse.IsSuccessStatusCode)
            {
                ErrorResponse errorResponse = await DeserialiseErrorResponse(httpResponse);
                throw new HttpRequestException(errorResponse.Message);
            }

            Response response = await DeserialiseResponse(httpResponse);

            if (!response.Success)
            {
                ErrorResponse errorResponse = await DeserialiseErrorResponse(httpResponse);
                throw new HttpRequestException(errorResponse.Message);
            }

            return httpResponse;
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

        static async Task<Response> DeserialiseResponse(HttpResponseMessage httpResponse)
        {
            string responseString = await httpResponse.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseString))
            {
                return new ErrorResponse($"Request failed with status code {(int)httpResponse.StatusCode} ({httpResponse.StatusCode})");
            }

            return responseString.FromJson<Response>();
        }

        static async Task<TResponse> DeserialiseSuccessResponse<TResponse>(HttpResponseMessage httpResponse)
        {
            string responseString = await httpResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TResponse>(responseString);
        }
    }
}
