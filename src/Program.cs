using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Extensions.Logging;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using TooGoodToGoNotifier.Api;
using TooGoodToGoNotifier.Configuration;

namespace TooGoodToGoNotifier
{
    public class Program
    {
        private static void Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            host.Services.UseScheduler(scheduler =>
            {
                var schedulerOptions = host.Services.GetService<IOptions<SchedulerOptions>>().Value;
                scheduler
                .Schedule<FavoriteBasketsWatcher>()
                .Cron(schedulerOptions.CronExpression)
                .PreventOverlapping(nameof(FavoriteBasketsWatcher));
            })
            .LogScheduledTaskProgress(host.Services.GetService<ILogger<IScheduler>>())
            .OnError((_) =>
            {
                host.StopAsync();
            });

            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                var config = configuration.Build();
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
                var timerOptions = host.Configuration.GetSection(nameof(SchedulerOptions)).Get<SchedulerOptions>();
                services.AddLogging()
                .AddScheduler()
                .Configure<SchedulerOptions>(host.Configuration.GetSection(nameof(SchedulerOptions)))
                .Configure<ApiOptions>(host.Configuration.GetSection(nameof(ApiOptions)))
                .Configure<EmailNotifierOptions>(host.Configuration.GetSection(nameof(EmailNotifierOptions)))
                .AddTransient<IRestClient, RestClient>(serviceProvider => GetRestClientInstance())
                .AddTransient<ITooGoodToGoApiService, TooGoodToGoApiService>()
                .AddTransient<IEmailNotifier, EmailNotifier>()
                .AddSingleton<FavoriteBasketsWatcher>();
            });

        private static RestClient GetRestClientInstance()
        {
            var restClient = new RestClient
            {
                ThrowOnAnyError = true
            };
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
            restClient.UseNewtonsoftJson(serializerSettings);
            return restClient;
        }
    }
}
