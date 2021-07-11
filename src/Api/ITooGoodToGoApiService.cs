using System.Threading.Tasks;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier.Api
{
    public interface ITooGoodToGoApiService
    {
        public Task<GetBasketsResponse> GetFavoriteBaskets();
    }
}