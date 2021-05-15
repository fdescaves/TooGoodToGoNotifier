using System;
using System.Threading;
using System.Threading.Tasks;
using Coravel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class TooGoodToGoNotifierWorker : BackgroundService
    {
        private readonly ILogger<TooGoodToGoNotifierWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SchedulerOptions _schedulerOptions;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public TooGoodToGoNotifierWorker(ILogger<TooGoodToGoNotifierWorker> logger, IHostApplicationLifetime hostApplicationLifetime, IServiceProvider serviceProvider, IOptions<SchedulerOptions> schedulerOptions)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _schedulerOptions = schedulerOptions.Value;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task<bool> CurrentTimeIsBetweenConfiguredRange()
            {
                var currentTime = DateTime.Now.TimeOfDay;
                return Task.FromResult(currentTime >= _schedulerOptions.StartTime && currentTime <= _schedulerOptions.EndTime);
            }

            _serviceProvider.UseScheduler(scheduler =>
            {
                scheduler.Schedule<FavoriteBasketsWatcher>()
                .EverySeconds(_schedulerOptions.Interval)
                .When(CurrentTimeIsBetweenConfiguredRange)
                .Zoned(TimeZoneInfo.Local)
                .PreventOverlapping(nameof(FavoriteBasketsWatcher));
            })
            .OnError((exception) =>
            {
                _logger.LogCritical(exception, "Critical error occured");
                _hostApplicationLifetime.StopApplication();
            });

            return Task.CompletedTask;
        }
    }
}
