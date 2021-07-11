using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using TooGoodToGoNotifier.Api.Requests;
using TooGoodToGoNotifier.Api.Responses;
using TooGoodToGoNotifier.Configuration;
using TooGoodToGoNotifier.Requests;

namespace TooGoodToGoNotifier.Api
{
    public class TooGoodToGoApiService : ITooGoodToGoApiService
    {
        private readonly ILogger _logger;
        private readonly ApiOptions _apiOptions;
        private readonly IRestClient _restClient;
        private readonly AuthenticationContext _authenticationContext;

        public TooGoodToGoApiService(ILogger<TooGoodToGoApiService> logger, IOptions<ApiOptions> apiOptions, IRestClient restClient, AuthenticationContext authenticationContext)
        {
            _logger = logger;
            _apiOptions = apiOptions.Value;
            _restClient = restClient;
            _authenticationContext = authenticationContext;
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

            var request = new RestRequest($"{_apiOptions.BaseUrl}{_apiOptions.GetItemsEndpoint}", Method.POST);
            request.AddHeader("authorization", $"Bearer {_authenticationContext.AccessToken}");

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

            request.AddJsonBody(getFavoriteBasketsRequest);

            var response = await ExecuteAsyncAndThrowIfNotSuccessful(request);

            var getBasketsResponse = _restClient.Deserialize<GetBasketsResponse>(response);

            return getBasketsResponse.Data;
        }

        private async Task Authenticate()
        {
            var request = new RestRequest($"{_apiOptions.BaseUrl}{_apiOptions.AuthenticateEndpoint}", Method.POST);

            var authenticationRequest = new AuthenticationRequest
            {
                DeviceType = "ANDROID",
                Email = _apiOptions.AuthenticationOptions.Email,
                Password = _apiOptions.AuthenticationOptions.Password
            };

            request.AddJsonBody(authenticationRequest);

            var response = await ExecuteAsyncAndThrowIfNotSuccessful(request);

            var authenticationResponse = _restClient.Deserialize<AuthenticationResponse>(response);

            _authenticationContext.AccessToken = authenticationResponse.Data.AccessToken;
            _authenticationContext.RefreshToken = authenticationResponse.Data.RefreshToken;
            _authenticationContext.UserId = authenticationResponse.Data.StartupData.User.UserId;
            _authenticationContext.LastAuthenticatedOn = DateTime.Now;
        }

        private async Task RefreshAccessToken()
        {
            var request = new RestRequest($"{_apiOptions.BaseUrl}{_apiOptions.RefreshTokenEndpoint}", Method.POST);

            var refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = _authenticationContext.RefreshToken
            };

            request.AddJsonBody(refreshTokenRequest);

            var response = await ExecuteAsyncAndThrowIfNotSuccessful(request);

            var refreshTokenResponse = _restClient.Deserialize<RefreshTokenResponse>(response);

            _authenticationContext.AccessToken = refreshTokenResponse.Data.AccessToken;
            _authenticationContext.RefreshToken = refreshTokenResponse.Data.RefreshToken;
            _authenticationContext.LastAuthenticatedOn = DateTime.Now;
        }

        private async Task<IRestResponse> ExecuteAsyncAndThrowIfNotSuccessful(IRestRequest restRequest)
        {
            var response = await _restClient.ExecuteAsync(restRequest);

            if (!response.IsSuccessful)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await Authenticate();
                    response = await _restClient.ExecuteAsync(restRequest);
                }
                else
                {
                    throw new TooGoodToGoRequestException("Error while requesting TooGoodToGo Api", response.StatusCode, response.Content, response.ErrorException);
                }
            }

            return response;
        }
    }
}
