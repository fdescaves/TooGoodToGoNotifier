using System;
using System.Threading.Tasks;
using Coravel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TooGoodToGo.Api.Interfaces;
using TooGoodToGo.Api.Models.Responses;
using TooGoodToGoNotifier.Core.Options;
using TooGoodToGoNotifier.Jobs;

namespace TooGoodToGoNotifier
{
    public static class WebApplicationExtensions
    {
        public static void ScheduleBackgroundJobs(this WebApplication app)
        {
            var notifierOptions = new NotifierOptions();
            app.Configuration.GetSection(nameof(NotifierOptions)).Bind(notifierOptions);

            app.Services.UseScheduler(scheduler =>
            {
                // Scheduling job to refresh access token
                scheduler.Schedule<RefreshAccessTokenJob>()
                .Cron(notifierOptions.RefreshAccessTokenCronExpression)
                .Zoned(TimeZoneInfo.Local);

                // Scheduling job to watch for available baskets
                scheduler.Schedule<FavoriteBasketsWatcherJob>()
                .EverySeconds(notifierOptions.ScanningInterval)
                .RunOnceAtStart()
                .When(() => CurrentTimeIsBetweenConfiguredRangeAsync(notifierOptions))
                .Zoned(TimeZoneInfo.Local)
                .PreventOverlapping(nameof(FavoriteBasketsWatcherJob));

                // Scheduling jobs to synchronize users favorites baskets
                scheduler.Schedule<SynchronizeFavoriteBasketsJob>()
                .Cron(notifierOptions.SynchronizeFavoriteBasketsCronExpression)
                .RunOnceAtStart()
                .PreventOverlapping(nameof(SynchronizeFavoriteBasketsJob))
                .Zoned(TimeZoneInfo.Local);
            })
            .OnError((exception) =>
            {
                app.Logger.LogCritical(exception, "Critical error occured");
                app.StopAsync();
            });
        }

        public static async void AuthenticateToTooGoodToGoServices(this WebApplication app)
        {
            try
            {
                app.Logger.LogInformation("Authenticating to TooGoodToGo's services");

                ITooGoodToGoService tooGoodToGoService = app.Services.GetService<ITooGoodToGoService>();

                AuthenticateByEmailResponse authenticateByEmailResponse = await tooGoodToGoService.AuthenticateByEmailAsync();

                int pollingAttempts = 0;
                AuthenticateByPollingIdResponse authenticateByPollingIdResponse;
                while (true)
                {
                    pollingAttempts++;
                    app.Logger.LogInformation("PollingId request attempt n°{pollingAttempts}", pollingAttempts);

                    authenticateByPollingIdResponse = await tooGoodToGoService.AuhenticateByPollingIdAsync(authenticateByEmailResponse.PollingId);

                    if (authenticateByPollingIdResponse != null)
                    {
                        Context context = app.Services.GetService<Context>();
                        context.AccessToken = authenticateByPollingIdResponse.AccessToken;
                        context.RefreshToken = authenticateByPollingIdResponse.RefreshToken;
                        context.TooGoodToGoUserId = authenticateByPollingIdResponse.StartupData.User.UserId;
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(15));
                }

                app.Logger.LogInformation("Successfuly authenticated to TooGoodToGo's services");
            }
            catch (Exception exception)
            {
                app.Logger.LogCritical(exception, "Critical error occured");
                await app.StopAsync();
            }
        }

        private static Task<bool> CurrentTimeIsBetweenConfiguredRangeAsync(NotifierOptions notifierOptions)
        {
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            return Task.FromResult(currentTime >= notifierOptions.StartTime && currentTime <= notifierOptions.EndTime);
        }
    }
}
