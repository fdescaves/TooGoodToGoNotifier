using System.Collections.Generic;
using System.Threading.Tasks;
using TooGoodToGoNotifier.Dto;

namespace TooGoodToGoNotifier.Interfaces
{
    public interface IBasketService
    {
        Task<IEnumerable<BasketDto>> GetFavoriteBasketsAsync();

        Task SetBasketAsFavoriteAsync(string id, bool isFavorite);
    }
}
