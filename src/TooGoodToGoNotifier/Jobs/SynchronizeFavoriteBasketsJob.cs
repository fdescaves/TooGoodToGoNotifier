using System;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TooGoodToGo.Api.Interfaces;
using TooGoodToGo.Api.Models.Responses;
using TooGoodToGoNotifier.Core.Options;
using TooGoodToGoNotifier.Entities;

namespace TooGoodToGoNotifier.Jobs
{
    public class SynchronizeFavoriteBasketsJob : IInvocable
    {
        private readonly ILogger<SynchronizeFavoriteBasketsJob> _logger;
        private readonly TooGoodToGoNotifierDbContext _dbContext;
        private readonly ITooGoodToGoService _tooGoodToGoService;
        private readonly Context _context;
        private readonly NotifierOptions _options;
        private readonly Guid _guid;

        public SynchronizeFavoriteBasketsJob(ILogger<SynchronizeFavoriteBasketsJob> logger, TooGoodToGoNotifierDbContext dbContext, ITooGoodToGoService tooGoodToGoService, Context context, IOptions<NotifierOptions> options)
        {
            _logger = logger;
            _dbContext = dbContext;
            _tooGoodToGoService = tooGoodToGoService;
            _context = context;
            _options = options.Value;
            _guid = Guid.NewGuid();
        }

        public async Task Invoke()
        {
            _logger.LogInformation($"{nameof(SynchronizeFavoriteBasketsJob)} started - {{Guid}}", _guid);

            GetBasketsResponse getBasketsResponse = await _tooGoodToGoService.GetFavoriteBasketsAsync(_context.AccessToken, _context.TooGoodToGoUserId);

            string[] currentlyFavoritedBaskets = getBasketsResponse.Items
                .Select(x => x.Item.ItemId)
                .ToArray();

            string[] userFavoritedBaskets = (await _dbContext.Users
                .ToArrayAsync())
                .SelectMany(x => x.FavoriteBaskets)
                .Distinct()
                .ToArray();

            await AddMissingBasketsToFavorites(currentlyFavoritedBaskets, userFavoritedBaskets);

            await RemoveUnusedBasketsFromFavorites(currentlyFavoritedBaskets, userFavoritedBaskets);

            _logger.LogInformation($"{nameof(SynchronizeFavoriteBasketsJob)} ended - {{Guid}}", _guid);
        }

        private async Task AddMissingBasketsToFavorites(string[] currentlyFavoritedBaskets, string[] userFavoritedBaskets)
        {
            string[] basketsToAdd = userFavoritedBaskets
                .Except(currentlyFavoritedBaskets)
                .ToArray();

            foreach (string basketId in basketsToAdd)
            {
                _logger.LogInformation("Removing from favorite basket with Id '{basketId}'", basketId);
                await _tooGoodToGoService.SetFavoriteAsync(_context.AccessToken, basketId, true);
                await Task.Delay(_options.ThrottleInterval);
            }
        }

        private async Task RemoveUnusedBasketsFromFavorites(string[] currentlyFavoritedBaskets, string[] userFavoritedBaskets)
        {
            string[] basketsToRemove = currentlyFavoritedBaskets
                .Except(userFavoritedBaskets)
                .ToArray();

            foreach (string basketId in basketsToRemove)
            {
                _logger.LogInformation("Adding as favorite basket with Id '{basketId}'", basketId);
                await _tooGoodToGoService.SetFavoriteAsync(_context.AccessToken, basketId, false);
                await Task.Delay(_options.ThrottleInterval);
            }
        }
    }
}
