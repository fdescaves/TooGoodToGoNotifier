using System;
using System.Net.Http;
using System.Net.Mime;
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
                await Authenticate();
            }
            else if (_authenticationContext.LastAuthenticatedOn.Value.AddHours(_apiOptions.RefreshTokenInterval) < DateTime.Now)
            {
                _logger.LogInformation($"{nameof(AuthenticationContext)} is expired => Refresh");
                await RefreshAccessToken();
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.GetItemsEndpoint}");
            request.Headers.Add("Authorization", $"Bearer {_authenticationContext.AccessToken}");

            // When FavoritesOnly is true, origin and radius are ignored but still must be specified.
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

            var response = await ExecuteAsyncAndThrowIfNotSuccessful(request);

            var getBasketsResponse = await DeserializeHttpResponseAsJson<GetBasketsResponse>(response);

            return getBasketsResponse;
        }

        public async Task Authenticate()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.AuthenticateEndpoint}");

            var authenticationRequest = new AuthenticationRequest
            {
                DeviceType = "ANDROID",
                Email = _apiOptions.AuthenticationOptions.Email,
                Password = _apiOptions.AuthenticationOptions.Password
            };

            SerializeHttpRequestContentAsJson(request, authenticationRequest);

            var response = await ExecuteAsyncAndThrowIfNotSuccessful(request);

            var authenticationResponse = await DeserializeHttpResponseAsJson<AuthenticationResponse>(response);

            _authenticationContext.AccessToken = authenticationResponse.AccessToken;
            _authenticationContext.RefreshToken = authenticationResponse.RefreshToken;
            _authenticationContext.UserId = authenticationResponse.StartupData.User.UserId;
            _authenticationContext.LastAuthenticatedOn = DateTime.Now;
        }

        private async Task RefreshAccessToken()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiOptions.BaseUrl}{_apiOptions.RefreshTokenEndpoint}");

            var refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = _authenticationContext.RefreshToken
            };

            SerializeHttpRequestContentAsJson(request, refreshTokenRequest);

            var response = await ExecuteAsyncAndThrowIfNotSuccessful(request);

            var refreshTokenResponse = await DeserializeHttpResponseAsJson<RefreshTokenResponse>(response);

            _authenticationContext.AccessToken = refreshTokenResponse.AccessToken;
            _authenticationContext.RefreshToken = refreshTokenResponse.RefreshToken;
            _authenticationContext.LastAuthenticatedOn = DateTime.Now;
        }

        private async Task<HttpResponseMessage> ExecuteAsyncAndThrowIfNotSuccessful(HttpRequestMessage httpRequestMessage)
        {
            var response = await _httpClient.SendAsync(httpRequestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new TooGoodToGoRequestException("Error while requesting TooGoodToGo Api", response.StatusCode, content);
            }

            return response;
        }

        private void SerializeHttpRequestContentAsJson(HttpRequestMessage httpRequestMessage, object content)
        {
            var serializedObject = JsonConvert.SerializeObject(content, _jsonSerializerSettings);
            httpRequestMessage.Content = new StringContent(serializedObject, Encoding.UTF8, MediaTypeNames.Application.Json);
        }

        private async Task<T> DeserializeHttpResponseAsJson<T>(HttpResponseMessage httpResponseMessage)
        {
            var stringContent = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(stringContent, _jsonSerializerSettings);
        }
    }
}
