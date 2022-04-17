using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Api.Responses;

namespace TooGoodToGoNotifier
{
    public class RefreshAccessTokenJob : IInvocable
    {
        private readonly ILogger _logger;
        private readonly ITooGoodToGoService _tooGoodToGoService;
        private readonly Context _context;
        private readonly Guid _guid;

        public RefreshAccessTokenJob(ILogger<RefreshAccessTokenJob> logger, ITooGoodToGoService tooGoodToGoService, Context context)
        {
            _logger = logger;
            _tooGoodToGoService = tooGoodToGoService;
            _context = context;
            _guid = Guid.NewGuid();
        }

        public async Task Invoke()
        {
            _logger.LogInformation($"{nameof(RefreshAccessTokenJob)} started - {{Guid}}", _guid);

            RefreshTokenResponse refreshTokenResponse = await _tooGoodToGoService.RefreshAccessTokenAsync(_context.RefreshToken);
            _context.AccessToken = refreshTokenResponse.AccessToken;
            _context.RefreshToken = refreshTokenResponse.RefreshToken;

            _logger.LogInformation($"{nameof(RefreshAccessTokenJob)} ended - {{Guid}}", _guid);
        }
    }
}
