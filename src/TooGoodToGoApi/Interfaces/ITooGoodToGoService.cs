using System.Threading.Tasks;
using TooGoodToGo.Api.Models.Responses;

namespace TooGoodToGo.Api.Interfaces
{
    public interface ITooGoodToGoService
    {
        public Task<GetBasketsResponse> GetFavoriteBasketsAsync(string accessToken, int userId);

        public Task<AuthenticateByEmailResponse> AuthenticateByEmailAsync();

        public Task<AuthenticateByPollingIdResponse> AuhenticateByPollingIdAsync(string pollingId);

        public Task<RefreshTokenResponse> RefreshAccessTokenAsync(string refreshToken);

        public Task SetFavoriteAsync(string accessToken, int basketId, bool isFavorite);
    }
}
