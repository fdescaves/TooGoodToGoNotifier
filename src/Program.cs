using System;
using System.Net;
using System.Net.Http;
using Coravel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class Program
    {
        private static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                IConfigurationRoot config = configuration.Build();
                LogManager.Setup().LoadConfigurationFromSection(config);
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders()
                .AddNLog();
            })
            .ConfigureServices((host, services) =>
            {
                services.AddLogging()
                .AddScheduler()
                .Configure<SchedulerOptions>(host.Configuration.GetSection(nameof(SchedulerOptions)))
                .Configure<ApiOptions>(host.Configuration.GetSection(nameof(ApiOptions)))
                .Configure<EmailNotifierOptions>(host.Configuration.GetSection(nameof(EmailNotifierOptions)))
                .AddTransient<ITooGoodToGoService, TooGoodToGoService>()
                .AddTransient<IEmailNotifier, EmailNotifier>()
                .AddSingleton<FavoriteBasketsWatcher>()
                .AddSingleton<AuthenticationContext>()
                .AddHostedService<TooGoodToGoNotifierWorker>();

                services.AddHttpClient<ITooGoodToGoService, TooGoodToGoService>(httpClient =>
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "TGTG/22.2.3 Dalvik/2.1.0 (Linux; U; Android 11; sdk_gphone_x86_arm Build/RSR1.201013.001)");
                })
                .AddPolicyHandler((serviceProvider, _) => HttpPolicyExtensions.HandleTransientHttpError()
                    .OrInner<TimeoutRejectedException>()
                    .OrResult(httpResponseMessage => httpResponseMessage.StatusCode == HttpStatusCode.TooManyRequests)
                    .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(30 * retryAttempt),
                    onRetry: (_, retryAttempt, timespan) =>
                    {
                        serviceProvider.GetService<ILogger<TooGoodToGoService>>().LogWarning($"Transient Http, timeout or too many attempts error occured: delaying for {timespan.TotalSeconds} seconds, then making retry n°{retryAttempt}");
                    })
                )
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10));
            });
    }
}
