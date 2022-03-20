using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TooGoodToGoNotifier.Api.Requests;
using TooGoodToGoNotifier.Api.Responses;
using TooGoodToGoNotifier.Configuration;
using TooGoodToGoNotifier.Requests;

namespace TooGoodToGoNotifier.Api
{
    public class TooGoodToGoService : ITooGoodToGoService
    {
        private readonly ILogger _logger;
        private readonly ApiOptions _apiOptions;
        private readonly HttpClient _httpClient;
        private readonly AuthenticationContext _authenticationContext;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public TooGoodToGoService(ILogger<TooGoodToGoService> logger, IOptions<ApiOptions> apiOptions, HttpClient httpClient, AuthenticationContext authenticationContext)
        {
            _logger = logger;
            _apiOptions = apiOptions.Value;
            _httpClient = httpClient;
            _authenticationContext = authenticationContext;
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new RequireObjectPropertiesContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        public async Task<GetBasketsResponse> GetFavoriteBaskets()
        {
            if (_authenticationContext.LastAuthenticatedOn is null)
            {
                _logger.LogInformation($"{nameof(AuthenticationContext.LastAuthenticatedOn)} is null => Authenticate");
                await AuthenticateByEmail();
            }
            else if (_authenticationContext.LastAuthenticatedOn.Value.AddHours(_apiOptions.RefreshTokenInterval) < DateTime.Now)
            {
                _logger.LogInformation($"{nameof(AuthenticationContext)} is expired => Refresh");
                await RefreshAccessToken();
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.GetItemsEndpoint}");
            request.Headers.Add("Authorization", $"Bearer {_authenticationContext.AccessToken}");

            // When FavoritesOnly is true, origin and radius are ignored but must still be specified.
            var getFavoriteBasketsRequest = new GetBasketsRequest
            {
                UserId = _authenticationContext.UserId.Value,
                Origin = new Origin
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

        public async Task AuthenticateByEmail()
        {
            _logger.LogInformation($"Starting email authentication procedure");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.AuthenticateByEmailEndpoint}");

            var authenticateByEmailRequest = new AuthenticateByEmailRequest
            {
                DeviceType = "ANDROID",
                Email = _apiOptions.AuthenticationOptions.Email
            };

            SerializeHttpRequestContentAsJson(request, authenticateByEmailRequest);

            AuthenticateByEmailResponse authenticateByEmailResponse = await ExecuteAndThrowIfNotSuccessfulAsync<AuthenticateByEmailResponse>(request);

            await AuhenticateByPollingId(authenticateByEmailRequest, authenticateByEmailResponse.PollingId);

            _logger.LogInformation($"Ending email authentication procedure");
        }

        private async Task AuhenticateByPollingId(AuthenticateByEmailRequest authenticateByEmailRequest, string pollingId)
        {
            int pollingAttempts = 0;
            AuthenticateByPollingIdResponse authenticateByPollingIdResponse;
            while (true)
            {
                pollingAttempts++;
                _logger.LogInformation("PollingId request attempt n°{pollingAttempts}", pollingAttempts);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.AuthenticateByRequestPollingIdEndpoint}");

                var authenticateByPollingIdRequest = new AuthenticateByPollingIdRequest
                {
                    DeviceType = authenticateByEmailRequest.DeviceType,
                    Email = authenticateByEmailRequest.Email,
                    RequestPollingId = pollingId
                };

                SerializeHttpRequestContentAsJson(request, authenticateByPollingIdRequest);

                authenticateByPollingIdResponse = await ExecuteAndThrowIfNotSuccessfulAsync<AuthenticateByPollingIdResponse>(request);

                if (authenticateByPollingIdResponse != null)
                {
                    _authenticationContext.AccessToken = authenticateByPollingIdResponse.AccessToken;
                    _authenticationContext.RefreshToken = authenticateByPollingIdResponse.RefreshToken;
                    _authenticationContext.UserId = authenticateByPollingIdResponse.StartupData.User.UserId;
                    _authenticationContext.LastAuthenticatedOn = DateTime.Now;
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }

        private async Task RefreshAccessToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.RefreshTokenEndpoint}");

            var refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = _authenticationContext.RefreshToken
            };

            SerializeHttpRequestContentAsJson(request, refreshTokenRequest);

            RefreshTokenResponse refreshTokenResponse = await ExecuteAndThrowIfNotSuccessfulAsync<RefreshTokenResponse>(request);

            _authenticationContext.AccessToken = refreshTokenResponse.AccessToken;
            _authenticationContext.RefreshToken = refreshTokenResponse.RefreshToken;
            _authenticationContext.LastAuthenticatedOn = DateTime.Now;
        }

        private async Task<T> ExecuteAndThrowIfNotSuccessfulAsync<T>(HttpRequestMessage httpRequestMessage)
        {
            HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);
            string httpResponseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new TooGoodToGoRequestException("Error while requesting TooGoodToGo Api", response.StatusCode, httpResponseContent);
            }

            return JsonConvert.DeserializeObject<T>(httpResponseContent, _jsonSerializerSettings);
        }

        private void SerializeHttpRequestContentAsJson(HttpRequestMessage httpRequestMessage, object content)
        {
            string serializedObject = JsonConvert.SerializeObject(content, _jsonSerializerSettings);
            httpRequestMessage.Content = new StringContent(serializedObject, Encoding.UTF8, "application/json");
        }
    }
}
