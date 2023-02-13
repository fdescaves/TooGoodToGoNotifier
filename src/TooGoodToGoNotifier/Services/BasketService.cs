using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TooGoodToGo.Api.Models;
using TooGoodToGoNotifier.Core;
using TooGoodToGoNotifier.Entities;
using TooGoodToGoNotifier.Interfaces;
using TooGoodToGoNotifier.Models;

namespace TooGoodToGoNotifier.Services
{
    public class BasketService : IBasketService
    {
        private readonly ILogger<BasketService> _logger;
        private readonly TooGoodToGoNotifierDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public BasketService(ILogger<BasketService> logger, TooGoodToGoNotifierDbContext dbContext, IMemoryCache memoryCache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<Basket>> GetFavoriteBasketsAsync(string email)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                throw new Exception("Unknown user");
            }

            if (_memoryCache.TryGetValue(Constants.BASKETS_CACHE_KEY, out List<TgtgBasket> baskets))
            {
                var userFavoritedBaskets = baskets
                    .Where(x => user.FavoriteBaskets.Contains(x.Item.ItemId))
                    .Select(x => new Basket
                    {
                        BasketId = x.Item.ItemId,
                        Name = x.Item.Name,
                        StoreName = x.Store.StoreName
                    })
                    .ToList();

                return userFavoritedBaskets;
            }

            return new List<Basket>();
        }

        public async Task UpdateBasketsFavoriteStatusAsync(string email, string[] basketIds, bool setAsFavorite)
        {
            User user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
            {
                throw new Exception("Unknown user");
            }

            foreach (string basketId in basketIds)
            {
                if (setAsFavorite && !user.FavoriteBaskets.Contains(basketId))
                {
                    user.FavoriteBaskets.Add(basketId);
                }
                else if (!setAsFavorite)
                {
                    user.FavoriteBaskets.Remove(basketId);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
