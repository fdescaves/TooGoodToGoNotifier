using Coravel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            CreateHostBuilder(args).Build().Run();
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
                services.AddLogging()
                .AddScheduler()
                .Configure<SchedulerOptions>(host.Configuration.GetSection(nameof(SchedulerOptions)))
                .Configure<ApiOptions>(host.Configuration.GetSection(nameof(ApiOptions)))
                .Configure<EmailNotifierOptions>(host.Configuration.GetSection(nameof(EmailNotifierOptions)))
                .AddTransient<IRestClient, RestClient>(serviceProvider => GetConfiguredRestClient())
                .AddTransient<ITooGoodToGoApiService, TooGoodToGoApiService>()
                .AddTransient<IEmailNotifier, EmailNotifier>()
                .AddSingleton<FavoriteBasketsWatcher>()
                .AddHostedService<TooGoodToGoNotifierWorker>();
            });

        private static RestClient GetConfiguredRestClient()
        {
            var restClient = new RestClient
            {
                ThrowOnDeserializationError = true
            };

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new RequireObjectPropertiesContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };

            restClient.UseNewtonsoftJson(serializerSettings);

            return restClient;
        }
    }
}
