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
    public class FavoriteBasketsWatcherJob : IInvocable
    {
        private readonly ILogger _logger;
        private readonly TooGoodToGoNotifierOptions _notifierOptions;
        private readonly ITooGoodToGoService _tooGoodToGoService;
        private readonly IEmailService _emailService;
        private readonly Context _context;
        private readonly Guid _guid;

        public FavoriteBasketsWatcherJob(ILogger<FavoriteBasketsWatcherJob> logger, IOptions<TooGoodToGoNotifierOptions> notifierOptions, ITooGoodToGoService tooGoodToGoService, IEmailService emailService, Context context)
        {
            _logger = logger;
            _notifierOptions = notifierOptions.Value;
            _tooGoodToGoService = tooGoodToGoService;
            _emailService = emailService;
            _context = context;
            _guid = Guid.NewGuid();
        }

        public async Task Invoke()
        {
            _logger.LogInformation($"{nameof(FavoriteBasketsWatcherJob)} started - {{Guid}}", _guid);

            GetBasketsResponse getBasketsResponse = await _tooGoodToGoService.GetFavoriteBasketsAsync(_context.AccessToken, _context.UserId);

            var basketsToNotify = new List<Basket>();
            foreach (Basket basket in getBasketsResponse.Items)
            {
                _logger.LogDebug("Basket N°{ItemId} | DisplayName: \"{DisplayName}\" | AvailableItems: {ItemsAvailable}", basket.Item.ItemId, basket.DisplayName, basket.ItemsAvailable);

                if (_context.NotifiedBaskets.TryGetValue(basket.Item.ItemId, out bool isAlreadyNotified))
                {
                    if (basket.ItemsAvailable > 0 && !isAlreadyNotified)
                    {
                        _logger.LogDebug("Basket N°{ItemId} restock will be notified", basket.Item.ItemId);
                        basketsToNotify.Add(basket);
                        _context.NotifiedBaskets[basket.Item.ItemId] = true;
                    }
                    else if (basket.ItemsAvailable == 0 && isAlreadyNotified)
                    {
                        _logger.LogDebug("Basket N°{ItemId} was previously notified and is now out of stock, notification will be reset", basket.Item.ItemId);
                        _context.NotifiedBaskets[basket.Item.ItemId] = false;
                    }
                }
                else if (basket.ItemsAvailable > 0)
                {
                    _logger.LogDebug("Basket N°{ItemId} is available for the first time, it will be notified", basket.Item.ItemId);
                    basketsToNotify.Add(basket);
                    _context.NotifiedBaskets.Add(basket.Item.ItemId, true);
                }
            }

            if (basketsToNotify.Count > 0 && _notifierOptions.Recipients.Length > 0)
            {
                _logger.LogInformation("{BasketsCount} basket(s) will be notified: {basketsToNotify}", basketsToNotify.Count, string.Join(" | ", basketsToNotify.Select(x => x.DisplayName)));

                var stringBuilder = new StringBuilder();
                foreach (var basket in basketsToNotify)
                {
                    stringBuilder.AppendLine($"{basket.ItemsAvailable} basket(s) available at \"{basket.DisplayName}\"");
                }

                _emailService.SendEmail("New basket(s)", stringBuilder.ToString(), _notifierOptions.Recipients);
            }

            _logger.LogInformation($"{nameof(FavoriteBasketsWatcherJob)} ended - {{Guid}}", _guid);
        }
    }
}
