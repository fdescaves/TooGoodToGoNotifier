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
            request.AddHeader("Content-Type", "application/json");

            var authenticationRequest = new AuthenticationRequest
            {
                DeviceType = "ANDROID",
                Email = _apiOptions.AuthenticationOptions.Email,
                Password = _apiOptions.AuthenticationOptions.Password
            };

            request.AddJsonBody(authenticationRequest);

            var response = await _restClient.ExecuteAsync<AuthenticationResponse>(request);

            if (!response.IsSuccessful)
            {
                throw new TooGoodToGoRequestException(response.Content);
            }

            return new AuthenticationContext
            {
                AccessToken = response.Data.AccessToken,
                RefreshToken = response.Data.RefreshToken,
                UserId = response.Data.StartupData.User.UserId
            };
        }

        public async Task<GetBasketsResponse> GetFavoriteBaskets(string accessToken, int userId)
        {
            var request = new RestRequest($"{_apiOptions.BaseUrl}{_apiOptions.GetItemsEndpoint}", Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("authorization", $"Bearer {accessToken}");

            var getFavoriteBasketsRequest = new GetBasketsRequest
            {
                UserId = userId,
                Origin = new Origin
                {
                    Latitude = 48.112427M,
                    Longitude = -1.661031M
                },
                Radius = 20,
                Page = 1,
                PageSize = 20,
                FavoritesOnly = true,
                WithStockOnly = false
            };

            request.AddJsonBody(getFavoriteBasketsRequest);

            var response = await _restClient.ExecuteAsync<GetBasketsResponse>(request);

            if (!response.IsSuccessful)
            {
                throw new TooGoodToGoRequestException(response.Content);
            }

            return response.Data;
        }
    }
}
