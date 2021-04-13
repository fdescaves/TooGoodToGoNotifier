using System.Collections.Generic;
using System.Timers;
using Microsoft.Extensions.Logging;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier
{
    public class FavoriteBasketsWatcher
    {
        private readonly ILogger _logger;
        private readonly ITooGoodToGoApiService _tooGoodToGoApiService;
        private readonly IEmailNotifier _emailNotifier;
        private readonly ITimer _timer;
        private readonly Dictionary<int, bool> _notifiedBaskets = new();

        private AuthenticationContext _authenticationContext;

        public FavoriteBasketsWatcher(ILogger<FavoriteBasketsWatcher> logger, ITimer timer, ITooGoodToGoApiService tooGoodToGoApiService, IEmailNotifier emailNotifier)
        {
            _logger = logger;
            _tooGoodToGoApiService = tooGoodToGoApiService;
            _emailNotifier = emailNotifier;
            _timer = timer;
            _timer.Elapsed += OnTimerElapsed;
        }

        public void Start()
        {
            _authenticationContext = _tooGoodToGoApiService.Authenticate();
            _timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var getBasketsResponse = _tooGoodToGoApiService.GetFavoriteBaskets(_authenticationContext.AccessToken, _authenticationContext.UserId);

            var basketsToNotify = new List<Basket>();
            foreach (var basket in getBasketsResponse.Items)
            {
                _logger.LogInformation($"Id: {basket.Item.ItemId} | DisplayName: \"{basket.DisplayName}\" | AvailableItems: {basket.AvailableItems}");

                if (_notifiedBaskets.TryGetValue(basket.Item.ItemId, out var isAlreadyNotified))
                {
                    if (basket.AvailableItems > 0 && !isAlreadyNotified)
                    {
                        _logger.LogInformation($"Basket N°{basket.Item.ItemId} restock will be notified.");
                        basketsToNotify.Add(basket);
                        _notifiedBaskets[basket.Item.ItemId] = true;
                    }
                    else if (basket.AvailableItems == 0 && isAlreadyNotified)
                    {
                        _logger.LogInformation($"Basket N°{basket.Item.ItemId} was previously notified and is now out of stock, notification will be reset.");
                        _notifiedBaskets[basket.Item.ItemId] = false;
                    }
                }
                else if (basket.AvailableItems > 0)
                {
                    _logger.LogInformation($"Basket N°{basket.Item.ItemId} is available for the first time, it will be notified.");
                    basketsToNotify.Add(basket);
                    _notifiedBaskets.Add(basket.Item.ItemId, true);
                }
            }

            if (basketsToNotify.Count > 0)
            {
                _emailNotifier.Notify(basketsToNotify);
            }

            _timer.Start();
        }
    }
}
