using System.Security.Authentication;
using System.Text.Json;
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

        public TooGoodToGoApiService(IOptions<ApiOptions> options, IRestClient restClient)
        {
            _apiOptions = options.Value;
            _restClient = restClient;
        }

        public AuthenticationContext Authenticate()
        {
            var request = new RestRequest($"{_apiOptions.BaseUrl}{_apiOptions.AuthenticateEndpoint}", Method.POST);
            request.AddHeader("Content-Type", "application/json");

            var authenticationRequest = new AuthenticationRequest
            {
                DeviceType = "ANDROID",
                Email = _apiOptions.AuthenticationOptions.Email,
                Password = _apiOptions.AuthenticationOptions.Password
            };

            request.AddJsonBody(JsonSerializer.Serialize(authenticationRequest));

            var response = _restClient.Execute<AuthenticationResponse>(request);

            if (!response.IsSuccessful)
            {
                throw new AuthenticationException(response.Content);
            }

            return new AuthenticationContext
            {
                AccessToken = response.Data.AccessToken,
                RefreshToken = response.Data.RefreshToken,
                UserId = response.Data.StartupData.User.UserId
            };
        }

        public GetBasketsResponse GetFavoriteBaskets(string accessToken, int userId)
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

            request.AddJsonBody(JsonSerializer.Serialize(getFavoriteBasketsRequest));

            var response = _restClient.Execute<GetBasketsResponse>(request);

            return response.Data;
        }
    }
}
