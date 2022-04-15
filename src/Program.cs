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
                var cookieContainer = new CookieContainer();

                services.AddLogging()
                .AddScheduler()
                .Configure<NotifierOptions>(host.Configuration.GetSection(nameof(NotifierOptions)))
                .Configure<TooGoodToGoApiOptions>(host.Configuration.GetSection(nameof(TooGoodToGoApiOptions)))
                .Configure<EmailServiceOptions>(host.Configuration.GetSection(nameof(EmailServiceOptions)))
                .AddTransient<ITooGoodToGoService, TooGoodToGoService>()
                .AddTransient<IEmailService, EmailService>()
                .AddSingleton<FavoriteBasketsWatcher>()
                .AddSingleton<TooGoodToGoApiContext>()
                .AddSingleton(cookieContainer)
                .AddHostedService<TooGoodToGoNotifierWorker>();

                services.AddHttpClient<ITooGoodToGoService, TooGoodToGoService>(httpClient =>
                {
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "TGTG/22.2.3 Dalvik/2.1.0 (Linux; U; Android 11; sdk_gphone_x86_arm Build/RSR1.201013.001)");
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        UseCookies = true,
                        CookieContainer = cookieContainer
                    };
                })
                .AddPolicyHandler((serviceProvider, _) => HttpPolicyExtensions.HandleTransientHttpError()
                    .OrInner<TimeoutRejectedException>()
                    .OrResult(httpResponseMessage => httpResponseMessage.StatusCode == HttpStatusCode.TooManyRequests)
                    .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(30 * retryAttempt),
                    onRetry: (_, retryAttempt, timespan) =>
                    {
                        serviceProvider.GetService<ILogger<TooGoodToGoService>>().LogWarning("Transient Http, timeout or too many attempts error occured: delaying for {seconds} seconds, then making retry n°{retryAttemptNumber}", timespan.TotalSeconds, retryAttempt);
                    })
                )
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10));
            });
    }
}
