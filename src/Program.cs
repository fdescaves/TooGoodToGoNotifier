using System;
using System.Net.Http;
using Coravel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Polly;
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
                logging
                .ClearProviders()
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

                services
                .AddHttpClient<ITooGoodToGoService, TooGoodToGoService>()
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10))
                .AddTransientHttpErrorPolicy(configurePolicy =>
                    configurePolicy
                    .OrInner<TimeoutRejectedException>()
                    .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(retryAttempt * 10)));
            });
    }
}
