using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TooGoodToGo.Api.Interfaces;
using TooGoodToGo.Api.Models;
using TooGoodToGoNotifier.Interfaces;
using TooGoodToGoNotifier.Models;

namespace TooGoodToGoNotifier.Services
{
    public class BasketService : IBasketService
    {
        private readonly ILogger<BasketService> _logger;
        private readonly TooGoodToGoNotifierContext _dbContext;
        private readonly Context _context;
        private readonly ITooGoodToGoService _tooGoodToGoService;

        public BasketService(ILogger<BasketService> logger, TooGoodToGoNotifierContext dbContext, Context context, ITooGoodToGoService tooGoodToGoService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _context = context;
            _tooGoodToGoService = tooGoodToGoService;
        }

        public async Task<IEnumerable<TgtgBasket>> GetFavoriteBasketsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task SetBasketAsFavoriteAsync(int id, bool isFavorite)
        {
            throw new NotImplementedException();
        }
    }
}
