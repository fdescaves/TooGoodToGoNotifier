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

            ThrowIfResponseIsNotSuccessFul(response);

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

            // When FavoritesOnly is true, origin and radius are ignored but still must be specified.
            var getFavoriteBasketsRequest = new GetBasketsRequest
            {
                UserId = userId,
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

            var response = await _restClient.ExecuteAsync<GetBasketsResponse>(request);

            ThrowIfResponseIsNotSuccessFul(response);

            return response.Data;
        }

        private static void ThrowIfResponseIsNotSuccessFul(IRestResponse response)
        {
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else if (!response.IsSuccessful)
            {
                throw new TooGoodToGoRequestException(response.Content);
            }
        }
    }
}
