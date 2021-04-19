using System;
using System.Threading;
using System.Threading.Tasks;
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class TooGoodToGoNotifierWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SchedulerOptions _schedulerOptions;

        public TooGoodToGoNotifierWorker(IServiceProvider serviceProvider, IOptions<SchedulerOptions> schedulerOptions)
        {
            _serviceProvider = serviceProvider;
            _schedulerOptions = schedulerOptions.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _serviceProvider.UseScheduler(scheduler =>
            {
                var scheduleInterval = scheduler.Schedule<FavoriteBasketsWatcher>();
                var scheduledEventConfiguration = scheduleInterval.Cron(_schedulerOptions.CronExpression);
                scheduledEventConfiguration
                .Zoned(TimeZoneInfo.Local)
                .PreventOverlapping(nameof(FavoriteBasketsWatcher));
            })
            .LogScheduledTaskProgress(_serviceProvider.GetService<ILogger<IScheduler>>())
            .OnError((exception) =>
            {
                throw exception;
            });

            return Task.CompletedTask;
        }
    }
}
