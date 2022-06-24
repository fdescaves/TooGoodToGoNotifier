using System.Collections.Generic;
using System.Threading.Tasks;
using TooGoodToGoNotifier.Models;

namespace TooGoodToGoNotifier.Interfaces
{
    public interface IBasketService
    {
        Task<IEnumerable<Basket>> GetFavoriteBasketsAsync(string userEmail);

        Task UpdateBasketsFavoriteStatusAsync(string userEmail, UpdateBasketsFavoriteStatusRequest request);
    }
}
