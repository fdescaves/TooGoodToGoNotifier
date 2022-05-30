using System;
using System.Threading;
using System.Threading.Tasks;
using Coravel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TooGoodToGo.Api.Interfaces;
using TooGoodToGo.Api.Models.Responses;
using TooGoodToGoNotifier.Core;
using TooGoodToGoNotifier.Jobs;

namespace TooGoodToGoNotifier
{
    public class TooGoodToGoNotifierWorker : BackgroundService
    {
        private readonly ILogger<TooGoodToGoNotifierWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly NotifierOptions _notifierOptions;
        private readonly ITooGoodToGoService _tooGoodToGoService;
        private readonly Context _context;

        public TooGoodToGoNotifierWorker(ILogger<TooGoodToGoNotifierWorker> logger, IHostApplicationLifetime hostApplicationLifetime, IServiceProvider serviceProvider, IOptions<NotifierOptions> notifierOptions, ITooGoodToGoService tooGoodToGoService, Context context)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hostApplicationLifetime = hostApplicationLifetime;
            _notifierOptions = notifierOptions.Value;
            _tooGoodToGoService = tooGoodToGoService;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await AuthenticateToTooGoodToGoServices();

            _serviceProvider.UseScheduler(scheduler =>
            {
                // Scheduling job to watch for available baskets
                scheduler.Schedule<FavoriteBasketsWatcherJob>()
                .EverySeconds(_notifierOptions.ScanningInterval)
                .When(CurrentTimeIsBetweenConfiguredRangeAsync)
                .Zoned(TimeZoneInfo.Local)
                .PreventOverlapping(nameof(FavoriteBasketsWatcherJob));

                // Scheduling job to refresh access token
                scheduler.Schedule<RefreshAccessTokenJob>()
                .Cron(_notifierOptions.RefreshAccessTokenCronExpression)
                .Zoned(TimeZoneInfo.Local);
            })
            .OnError((exception) =>
            {
                _logger.LogCritical(exception, "Critical error occured");
                _hostApplicationLifetime.StopApplication();
            });
        }

        private async Task AuthenticateToTooGoodToGoServices()
        {
            _logger.LogInformation("TooGoodToGoNotifier isn't authenticated, authenticating to TooGoodToGo's services");

            AuthenticateByEmailResponse authenticateByEmailResponse = await _tooGoodToGoService.AuthenticateByEmailAsync();

            int pollingAttempts = 0;
            AuthenticateByPollingIdResponse authenticateByPollingIdResponse;
            while (true)
            {
                pollingAttempts++;
                _logger.LogInformation("PollingId request attempt n°{pollingAttempts}", pollingAttempts);

                authenticateByPollingIdResponse = await _tooGoodToGoService.AuhenticateByPollingIdAsync(authenticateByEmailResponse.PollingId);

                if (authenticateByPollingIdResponse != null)
                {
                    _context.AccessToken = authenticateByPollingIdResponse.AccessToken;
                    _context.RefreshToken = authenticateByPollingIdResponse.RefreshToken;
                    _context.TooGoodToGoUserId = authenticateByPollingIdResponse.StartupData.User.UserId;
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(15));
            }

            _logger.LogInformation("Ended authenticating to TooGoodToGo's services");
        }

        private Task<bool> CurrentTimeIsBetweenConfiguredRangeAsync()
        {
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            return Task.FromResult(currentTime >= _notifierOptions.StartTime && currentTime <= _notifierOptions.EndTime);
        }
    }
}
