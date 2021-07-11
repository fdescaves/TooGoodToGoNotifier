using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier
{
    public class FavoriteBasketsWatcher : IInvocable
    {
        private readonly ILogger _logger;
        private readonly ITooGoodToGoApiService _tooGoodToGoApiService;
        private readonly IEmailNotifier _emailNotifier;
        private readonly Dictionary<int, bool> _notifiedBaskets = new();

        public FavoriteBasketsWatcher(ILogger<FavoriteBasketsWatcher> logger, ITooGoodToGoApiService tooGoodToGoApiService, IEmailNotifier emailNotifier)
        {
            _logger = logger;
            _tooGoodToGoApiService = tooGoodToGoApiService;
            _emailNotifier = emailNotifier;
        }

        public async Task Invoke()
        {
            _logger.LogInformation($"{nameof(FavoriteBasketsWatcher)} started");

            var getBasketsResponse = await _tooGoodToGoApiService.GetFavoriteBaskets();

            var basketsToNotify = new List<Basket>();
            foreach (var basket in getBasketsResponse.Items)
            {
                _logger.LogDebug($"Basket N°{basket.Item.ItemId} | DisplayName: \"{basket.DisplayName}\" | AvailableItems: {basket.ItemsAvailable}");

                if (_notifiedBaskets.TryGetValue(basket.Item.ItemId, out var isAlreadyNotified))
                {
                    if (basket.ItemsAvailable > 0 && !isAlreadyNotified)
                    {
                        _logger.LogDebug($"Basket N°{basket.Item.ItemId} restock will be notified.");
                        basketsToNotify.Add(basket);
                        _notifiedBaskets[basket.Item.ItemId] = true;
                    }
                    else if (basket.ItemsAvailable == 0 && isAlreadyNotified)
                    {
                        _logger.LogDebug($"Basket N°{basket.Item.ItemId} was previously notified and is now out of stock, notification will be reset.");
                        _notifiedBaskets[basket.Item.ItemId] = false;
                    }
                }
                else if (basket.ItemsAvailable > 0)
                {
                    _logger.LogDebug($"Basket N°{basket.Item.ItemId} is available for the first time, it will be notified.");
                    basketsToNotify.Add(basket);
                    _notifiedBaskets.Add(basket.Item.ItemId, true);
                }
            }

            if (basketsToNotify.Count > 0)
            {
                _logger.LogInformation($"{basketsToNotify.Count} basket(s) will be notified: {string.Join("| ", basketsToNotify.Select(x => x.DisplayName))}");
                _emailNotifier.Notify(basketsToNotify);
            }

            _logger.LogInformation($"{nameof(FavoriteBasketsWatcher)} ended");
        }
    }
}
