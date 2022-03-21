using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Api.Responses;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class FavoriteBasketsWatcher : IInvocable
    {
        private readonly ILogger _logger;
        private readonly NotifierOptions _notifierOptions;
        private readonly ITooGoodToGoService _tooGoodToGoService;
        private readonly IEmailService _emailService;
        private readonly Dictionary<int, bool> _notifiedBaskets;
        private readonly Guid _guid;

        public FavoriteBasketsWatcher(ILogger<FavoriteBasketsWatcher> logger, IOptions<NotifierOptions> notifierOptions, ITooGoodToGoService tooGoodToGoService, IEmailService emailService)
        {
            _logger = logger;
            _notifierOptions = notifierOptions.Value;
            _tooGoodToGoService = tooGoodToGoService;
            _emailService = emailService;
            _notifiedBaskets = new();
            _guid = Guid.NewGuid();
        }

        public async Task Invoke()
        {
            _logger.LogInformation($"{nameof(FavoriteBasketsWatcher)} started - {{Guid}}", _guid);

            GetBasketsResponse getBasketsResponse = await _tooGoodToGoService.GetFavoriteBaskets();

            var basketsToNotify = new List<Basket>();
            foreach (Basket basket in getBasketsResponse.Items)
            {
                _logger.LogDebug("Basket N°{ItemId} | DisplayName: \"{DisplayName}\" | AvailableItems: {ItemsAvailable}", basket.Item.ItemId, basket.DisplayName, basket.ItemsAvailable);

                if (_notifiedBaskets.TryGetValue(basket.Item.ItemId, out bool isAlreadyNotified))
                {
                    if (basket.ItemsAvailable > 0 && !isAlreadyNotified)
                    {
                        _logger.LogDebug("Basket N°{ItemId} restock will be notified.", basket.Item.ItemId);
                        basketsToNotify.Add(basket);
                        _notifiedBaskets[basket.Item.ItemId] = true;
                    }
                    else if (basket.ItemsAvailable == 0 && isAlreadyNotified)
                    {
                        _logger.LogDebug("Basket N°{ItemId} was previously notified and is now out of stock, notification will be reset.", basket.Item.ItemId);
                        _notifiedBaskets[basket.Item.ItemId] = false;
                    }
                }
                else if (basket.ItemsAvailable > 0)
                {
                    _logger.LogDebug("Basket N°{ItemId} is available for the first time, it will be notified.", basket.Item.ItemId);
                    basketsToNotify.Add(basket);
                    _notifiedBaskets.Add(basket.Item.ItemId, true);
                }
            }

            if (basketsToNotify.Count > 0)
            {
                _logger.LogInformation("{BasketsCount} basket(s) will be notified: {basketsToNotify}", basketsToNotify.Count, string.Join(" | ", basketsToNotify.Select(x => x.DisplayName)));

                var stringBuilder = new StringBuilder();
                foreach (var basket in basketsToNotify)
                {
                    stringBuilder.AppendLine($"{basket.ItemsAvailable} basket(s) available at \"{basket.DisplayName}\"");
                }

                _emailService.SendEmail("New available basket(s)", stringBuilder.ToString(), _notifierOptions.Recipients);
            }

            _logger.LogInformation($"{nameof(FavoriteBasketsWatcher)} ended - {{Guid}}", _guid);
        }
    }
}
