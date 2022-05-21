﻿using System;
using System.Collections.Generic;
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
        private readonly NotifierOptions _notifierOptions;
        private readonly ITooGoodToGoService _tooGoodToGoService;
        private readonly IEmailService _emailService;
        private readonly Context _context;
        private readonly Guid _guid;

        public FavoriteBasketsWatcherJob(ILogger<FavoriteBasketsWatcherJob> logger, IOptions<NotifierOptions> notifierOptions, ITooGoodToGoService tooGoodToGoService, IEmailService emailService, Context context)
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
                if (_context.NotifiedBaskets.TryGetValue(basket.Item.ItemId, out bool isAlreadyNotified))
                {
                    if (basket.ItemsAvailable > 0 && !isAlreadyNotified)
                    {
                        await NotifyBasket(basket);
                        _context.NotifiedBaskets[basket.Item.ItemId] = true;
                    }
                    else if (basket.ItemsAvailable == 0 && isAlreadyNotified)
                    {
                        _context.NotifiedBaskets[basket.Item.ItemId] = false;
                    }
                }
                else if (basket.ItemsAvailable > 0)
                {
                    await NotifyBasket(basket);
                    _context.NotifiedBaskets.Add(basket.Item.ItemId, true);
                }
            }

            _logger.LogInformation($"{nameof(FavoriteBasketsWatcherJob)} ended - {{Guid}}", _guid);
        }

        private async Task NotifyBasket(Basket basket)
        {
            if (_notifierOptions.SubscribedRecipientsByBasketId.TryGetValue(basket.Item.ItemId.ToString(), out string[] recipients) && recipients.Length > 0)
            {
                _logger.LogInformation("{basketToNotify} will be notified to: {Recipients}", basket.DisplayName, recipients);
                await _emailService.SendEmailAsync("New basket(s)", $"{basket.ItemsAvailable} basket(s) available at \"{basket.DisplayName}\"", recipients);
            }
            else
            {
                _logger.LogWarning("{basketToNotifyId} isn't filtered, email notifications won't be sent", basket.Item.ItemId);
            }
        }
    }
}
