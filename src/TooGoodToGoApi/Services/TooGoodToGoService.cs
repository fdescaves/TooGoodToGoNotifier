using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TooGoodToGo.Api.Interfaces;
using TooGoodToGo.Api.Models;
using TooGoodToGo.Api.Models.Requests;
using TooGoodToGo.Api.Models.Responses;
using TooGoodToGoNotifier.Core;

namespace TooGoodToGo.Api.Services
{
    public class TooGoodToGoService : ITooGoodToGoService
    {
        private readonly ILogger _logger;
        private readonly TooGoodToGoApiOptions _apiOptions;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public TooGoodToGoService(ILogger<TooGoodToGoService> logger, IOptions<TooGoodToGoApiOptions> apiOptions, HttpClient httpClient)
        {
            _logger = logger;
            _apiOptions = apiOptions.Value;
            _httpClient = httpClient;
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        public async Task<GetBasketsResponse> GetFavoriteBasketsAsync(string accessToken, int userId)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.GetItemsEndpoint}");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            // When FavoritesOnly is true, origin and radius are ignored but must still be specified.
            var getFavoriteBasketsRequest = new GetBasketsRequest
            {
                UserId = userId,
                Origin = new TgtgLocation
                {
                    Latitude = 0,
                    Longitude = 0
                },
                Radius = 1,
                Page = 1,
                PageSize = 400, // Max page size allowed by TooGoodToGo API
                FavoritesOnly = true,
                WithStockOnly = false
            };

            SerializeHttpRequestContentAsJson(request, getFavoriteBasketsRequest);

            GetBasketsResponse getBasketsResponse = await ExecuteAndThrowIfNotSuccessfulAsync<GetBasketsResponse>(request);

            return getBasketsResponse;
        }

        public async Task<AuthenticateByEmailResponse> AuthenticateByEmailAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.AuthenticateByEmailEndpoint}");

            var authenticateByEmailRequest = new AuthenticateByEmailRequest
            {
                DeviceType = "ANDROID",
                Email = _apiOptions.AccountEmail
            };

            SerializeHttpRequestContentAsJson(request, authenticateByEmailRequest);

            AuthenticateByEmailResponse authenticateByEmailResponse = await ExecuteAndThrowIfNotSuccessfulAsync<AuthenticateByEmailResponse>(request);

            return authenticateByEmailResponse;
        }

        public async Task<AuthenticateByPollingIdResponse> AuhenticateByPollingIdAsync(string pollingId)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.AuthenticateByRequestPollingIdEndpoint}");

            var authenticateByPollingIdRequest = new AuthenticateByPollingIdRequest
            {
                DeviceType = "ANDROID",
                Email = _apiOptions.AccountEmail,
                RequestPollingId = pollingId
            };

            SerializeHttpRequestContentAsJson(request, authenticateByPollingIdRequest);

            AuthenticateByPollingIdResponse authenticateByPollingIdResponse = await ExecuteAndThrowIfNotSuccessfulAsync<AuthenticateByPollingIdResponse>(request);

            return authenticateByPollingIdResponse;
        }

        public async Task<RefreshTokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.RefreshTokenEndpoint}");

            var refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            };

            SerializeHttpRequestContentAsJson(request, refreshTokenRequest);

            RefreshTokenResponse refreshTokenResponse = await ExecuteAndThrowIfNotSuccessfulAsync<RefreshTokenResponse>(request);

            return refreshTokenResponse;
        }

        public async Task SetFavoriteAsync(string accessToken, int basketId, bool isFavorite)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.GetItemsEndpoint}{basketId}/setFavorite");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var setBasketFavoriteStatusRequest = new SetBasketFavoriteStatusRequest
            {
                IsFavorite = isFavorite
            };

            SerializeHttpRequestContentAsJson(request, setBasketFavoriteStatusRequest);

            await ExecuteAndThrowIfNotSuccessfulAsync(request);
        }

        private async Task<T> ExecuteAndThrowIfNotSuccessfulAsync<T>(HttpRequestMessage httpRequestMessage)
        {
            string httpResponseContent = await ExecuteAndThrowIfNotSuccessfulAsync(httpRequestMessage);

            return JsonConvert.DeserializeObject<T>(httpResponseContent, _jsonSerializerSettings);
        }

        private async Task<string> ExecuteAndThrowIfNotSuccessfulAsync(HttpRequestMessage httpRequestMessage)
        {
            HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

            string httpResponseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new TooGoodToGoRequestException("Error while requesting TooGoodToGo's services", response.StatusCode, httpResponseContent);
            }

            return httpResponseContent;
        }

        private void SerializeHttpRequestContentAsJson(HttpRequestMessage httpRequestMessage, object content)
        {
            string serializedObject = JsonConvert.SerializeObject(content, _jsonSerializerSettings);
            httpRequestMessage.Content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
        }
    }
}
