using System.Threading.Tasks;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier.Api
{
    public interface ITooGoodToGoApiService
    {
        public Task<AuthenticationContext> Authenticate();

        public Task<AuthenticationContext> RefreshAccessToken(AuthenticationContext authenticationContext);

        public Task<GetBasketsResponse> GetFavoriteBaskets(AuthenticationContext authenticationContext);
    }
}