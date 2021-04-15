using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        private static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            var favoriteBasketsWatcher = host.Services.GetRequiredService<FavoriteBasketsWatcher>();
            favoriteBasketsWatcher.Start();

            await host.RunAsync();
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
                var timerOptions = host.Configuration.GetSection(nameof(TimerOptions)).Get<TimerOptions>();
                services.AddLogging()
                .Configure<ApiOptions>(host.Configuration.GetSection(nameof(ApiOptions)))
                .Configure<AuthenticationOptions>(host.Configuration.GetSection(nameof(AuthenticationOptions)))
                .AddSingleton<ITimer, TooGoodToGoTimer>(serviceProvider => new TooGoodToGoTimer { AutoReset = false, Interval = timerOptions.Interval })
                .AddSingleton<IRestClient, RestClient>(serviceProvider => GetRestClientInstance())
                .AddSingleton<ITooGoodToGoApiService, TooGoodToGoApiService>()
                .AddSingleton<IEmailNotifier, EmailNotifier>()
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
