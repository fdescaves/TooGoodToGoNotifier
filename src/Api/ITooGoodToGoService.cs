using System.Threading.Tasks;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier.Api
{
    public interface ITooGoodToGoService
    {
        public Task<GetBasketsResponse> GetFavoriteBasketsAsync(string accessToken, int userId);

        public Task<AuthenticateByEmailResponse> AuthenticateByEmailAsync();

        public Task<AuthenticateByPollingIdResponse> AuhenticateByPollingIdAsync(string pollingId);

        public Task<RefreshTokenResponse> RefreshAccessTokenAsync(string refreshToken);
    }
}
