using System;
using System.Threading.Tasks;
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
        private readonly IRestClient _restClient;
        private readonly ApiOptions _apiOptions;

        public TooGoodToGoApiService(IOptions<ApiOptions> apiOptions, IRestClient restClient)
        {
            _apiOptions = apiOptions.Value;
            _restClient = restClient;
        }

        public async Task<AuthenticationContext> Authenticate()
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

            return new AuthenticationContext
            {
                AccessToken = authenticationResponse.Data.AccessToken,
                RefreshToken = authenticationResponse.Data.RefreshToken,
                UserId = authenticationResponse.Data.StartupData.User.UserId,
                AuthenticatedOn = DateTime.Now
            };
        }

        public async Task<AuthenticationContext> RefreshAccessToken(AuthenticationContext authenticationContext)
        {
            var request = new RestRequest($"{_apiOptions.BaseUrl}{_apiOptions.RefreshTokenEndpoint}", Method.POST);

            var refreshTokenRequest = new RefreshTokenRequest
            {
                RefreshToken = authenticationContext.RefreshToken
            };

            request.AddJsonBody(refreshTokenRequest);

            var response = await ExecuteAsyncAndThrowIfNotSuccessful(request);

            var refreshTokenResponse = _restClient.Deserialize<RefreshTokenResponse>(response);

            return new AuthenticationContext
            {
                AccessToken = refreshTokenResponse.Data.AccessToken,
                RefreshToken = refreshTokenResponse.Data.RefreshToken,
                UserId = authenticationContext.UserId,
                AuthenticatedOn = DateTime.Now
            };
        }

        public async Task<GetBasketsResponse> GetFavoriteBaskets(AuthenticationContext authenticationContext)
        {
            var request = new RestRequest($"{_apiOptions.BaseUrl}{_apiOptions.GetItemsEndpoint}", Method.POST);
            request.AddHeader("authorization", $"Bearer {authenticationContext.AccessToken}");

            // When FavoritesOnly is true, origin and radius are ignored but still must be specified.
            var getFavoriteBasketsRequest = new GetBasketsRequest
            {
                UserId = authenticationContext.UserId,
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

        private async Task<IRestResponse> ExecuteAsyncAndThrowIfNotSuccessful(IRestRequest restRequest)
        {
            var response = await _restClient.ExecuteAsync(restRequest);

            if (!response.IsSuccessful)
            {
                throw new TooGoodToGoRequestException("Error while requesting TooGoodToGo Api", response.StatusCode, response.Content, response.ErrorException);
            }

            return response;
        }
    }
}
