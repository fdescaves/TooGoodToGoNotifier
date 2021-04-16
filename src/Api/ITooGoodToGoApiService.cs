using System.Threading.Tasks;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier.Api
{
    public interface ITooGoodToGoApiService
    {
        public Task<AuthenticationContext> Authenticate();

        public Task<GetBasketsResponse> GetFavoriteBaskets(string accessToken, int userId);
    }
}