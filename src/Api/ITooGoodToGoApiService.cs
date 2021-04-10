using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier.Api
{
    public interface ITooGoodToGoApiService
    {
        public AuthenticationContext Authenticate();

        public GetBasketsResponse GetFavoriteBaskets(string accessToken, int userId);
    }
}