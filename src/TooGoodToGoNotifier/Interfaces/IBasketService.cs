using System.Collections.Generic;
using System.Threading.Tasks;
using TooGoodToGo.Api.Models;

namespace TooGoodToGoNotifier.Interfaces
{
    public interface IBasketService
    {
        Task<IEnumerable<TgtgBasket>> GetFavoriteBasketsAsync();

        Task SetBasketAsFavoriteAsync(int id, bool isFavorite);
    }
}
