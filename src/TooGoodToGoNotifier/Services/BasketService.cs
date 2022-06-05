using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TooGoodToGo.Api.Models;
using TooGoodToGoNotifier.Core;
using TooGoodToGoNotifier.Dto;
using TooGoodToGoNotifier.Interfaces;
using TooGoodToGoNotifier.Models;

namespace TooGoodToGoNotifier.Services
{
    public class BasketService : IBasketService
    {
        private readonly ILogger<BasketService> _logger;
        private readonly TooGoodToGoNotifierContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public BasketService(ILogger<BasketService> logger, TooGoodToGoNotifierContext dbContext, IMemoryCache memoryCache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<BasketDto>> GetFavoriteBasketsAsync()
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync();

            if (_memoryCache.TryGetValue(Constants.BASKETS_CACHE_KEY, out List<TgtgBasket> baskets))
            {
                var userFavoritedBaskets = baskets
                    .Where(x => user.FavoriteBaskets.Contains(x.Item.ItemId))
                    .Select(x => new BasketDto
                    {
                        BasketId = x.Item.ItemId,
                        Name = x.Item.Name,
                        StoreName = x.Store.StoreName
                    })
                    .ToList();

                return userFavoritedBaskets;
            }

            return new List<BasketDto>();
        }

        public async Task SetBasketAsFavoriteAsync(string id, bool isFavorite)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync();

            if (isFavorite && !user.FavoriteBaskets.Contains(id))
            {
                user.FavoriteBaskets.Add(id);
            }
            else if (!isFavorite)
            {
                user.FavoriteBaskets.Remove(id);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
