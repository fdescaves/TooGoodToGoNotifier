using System.Collections.Generic;
using System.Threading.Tasks;
using TooGoodToGoNotifier.Dto;

namespace TooGoodToGoNotifier.Interfaces
{
    public interface IBasketService
    {
        Task<IEnumerable<BasketDto>> GetFavoriteBasketsAsync(string userEmail);

        Task SetBasketAsFavoriteAsync(string userEmail, string id, bool isFavorite);
    }
}
