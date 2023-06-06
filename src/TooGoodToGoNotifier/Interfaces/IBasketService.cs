using System.Collections.Generic;
using System.Threading.Tasks;
using TooGoodToGoNotifier.Models;

namespace TooGoodToGoNotifier.Interfaces
{
    public interface IBasketService
    {
        Task<IEnumerable<Basket>> GetFavoriteBasketsAsync(string email);

        Task UpdateBasketsFavoriteStatusAsync(string email, string[] basketIds, bool setAsFavorite);
    }
}
