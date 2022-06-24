using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using TooGoodToGo.Api.Interfaces;
using TooGoodToGo.Api.Models;
using TooGoodToGo.Api.Models.Responses;
using TooGoodToGoNotifier.Entities;
using TooGoodToGoNotifier.Interfaces;

namespace TooGoodToGoNotifier.Jobs
{
    public class FavoriteBasketsWatcherJob : IInvocable
    {
        private readonly ILogger _logger;
        private readonly ITooGoodToGoService _tooGoodToGoService;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        private readonly Context _context;
        private readonly Guid _guid;
        private List<User> _users;

        public FavoriteBasketsWatcherJob(ILogger<FavoriteBasketsWatcherJob> logger, ITooGoodToGoService tooGoodToGoService, IEmailService emailService,
            IUserService userService, Context context)
        {
            _logger = logger;
            _tooGoodToGoService = tooGoodToGoService;
            _emailService = emailService;
            _userService = userService;
            _context = context;
            _guid = Guid.NewGuid();
        }

        public async Task Invoke()
        {
            _logger.LogInformation($"{nameof(FavoriteBasketsWatcherJob)} started - {{Guid}}", _guid);

            _users = await _userService.GetAllUsersAsync();
            GetBasketsResponse getBasketsResponse = await _tooGoodToGoService.GetFavoriteBasketsAsync(_context.AccessToken, _context.TooGoodToGoUserId);

            var basketsToNotify = new List<TgtgBasket>();
            foreach (TgtgBasket basket in getBasketsResponse.Items)
            {
                if (_context.NotifiedBaskets.TryGetValue(basket.Item.ItemId, out bool isAlreadyNotified))
                {
                    if (basket.ItemsAvailable > 0 && !isAlreadyNotified)
                    {
                        await NotifyBasketAsync(basket);
                        _context.NotifiedBaskets[basket.Item.ItemId] = true;
                    }
                    else if (basket.ItemsAvailable == 0 && isAlreadyNotified)
                    {
                        _context.NotifiedBaskets[basket.Item.ItemId] = false;
                    }
                }
                else if (basket.ItemsAvailable > 0)
                {
                    await NotifyBasketAsync(basket);
                    _context.NotifiedBaskets.Add(basket.Item.ItemId, true);
                }
            }

            _logger.LogInformation($"{nameof(FavoriteBasketsWatcherJob)} ended - {{Guid}}", _guid);
        }

        private async Task NotifyBasketAsync(TgtgBasket basket)
        {
            string[] recipients = _users
                .Where(x => x.FavoriteBaskets.Contains(basket.Item.ItemId))
                .Select(x => x.Email)
                .ToArray();

            if (recipients.Length > 0)
            {
                _logger.LogInformation("{basketToNotify} will be notified to: {Recipients}", basket.DisplayName, recipients);
                await _emailService.SendEmailAsync("New basket(s)", $"{basket.ItemsAvailable} basket(s) available at \"{basket.DisplayName}\"", recipients);
            }
            else
            {
                _logger.LogWarning("Default recipients aren't configured, {basketToNotifyId} - {basketDisplayName} won't be notified", basket.Item.ItemId, basket.DisplayName);
            }
        }
    }
}
