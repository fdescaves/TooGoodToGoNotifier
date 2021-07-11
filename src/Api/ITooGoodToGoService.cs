using System.Threading.Tasks;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier.Api
{
    public interface ITooGoodToGoService
    {
        public Task<GetBasketsResponse> GetFavoriteBaskets();
    }
}