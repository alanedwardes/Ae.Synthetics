using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using InfluxDB.Client;
using Microsoft.Extensions.DependencyInjection;
using Ae.Synthetics.Alerting.InfluxDb;
using System;
using System.Net.Http;
using Polly;
using Ae.Synthetics.Runner;
using System.Threading;
using Ae.Synthetics.Alerting;
using Microsoft.Extensions.Logging;
using Ae.Synthetics.Console.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace Ae.Synthetics.Console
{
    class Program
    {
        static void Main(string[] args) => DoWork(args).GetAwaiter().GetResult();

        private static async Task DoWork(string[] args)
        {
            var configuration =  JsonSerializer.Deserialize<SyntheticsConfiguration>(await File.ReadAllTextAsync("config.json"));

            var services = new ServiceCollection();

            services.AddSyntheticsRunner(new SyntheticsRunnerConfig
            {
                SyntheticTestTimeout = TimeSpan.FromSeconds(15)
            });

            services.AddLogging(x => x.AddConsole());

            foreach (var sink in configuration.Sinks)
            {
                switch (sink.Type.ToLower())
                {
                    case "influxdb":
                        ConfigureInfluxDbSink(services, sink.Configuration.Deserialize<InfluxDbSinkConfiguration>());
                        break;
                    case "logger":
                        services.AddSingleton<ISyntheticsAlerter>(x => new SyntheticsLoggerAlerter(x.GetRequiredService<ILogger<SyntheticsLoggerAlerter>>()));
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown sink {sink.Type}");
                }
            }

            foreach (var test in configuration.Tests)
            {
                switch (test.Type.ToLower())
                {
                    case "http":
                        ConfigureHttpTest(services, test.Configuration.Deserialize<HttpSyntheticTestConfiguration>());
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown test {test.Type}");
                }
            }

            services.AddHttpClient(HttpSyntheticTest.CLIENT_NAME)
                    .ConfigurePrimaryHttpMessageHandler(x => new SocketsHttpHandler
                    {
                        AllowAutoRedirect = false
                    })
                    .AddTransientHttpErrorPolicy(x =>
                    {
                        return x.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                    });

            services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

            services.BuildServiceProvider()
                    .GetRequiredService<ISyntheticsRunner>()
                    .RunSyntheticTestsForever(configuration.Interval, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
        }

        private static void ConfigureInfluxDbSink(IServiceCollection services, InfluxDbSinkConfiguration configuration)
        {
            var influxOptions = InfluxDBClientOptions.Builder.CreateNew()
                .Url(configuration.Url)
                .Bucket(configuration.Bucket)
                .Org(configuration.Org)
                .AuthenticateToken(configuration.Token)
                .Build();

            var influxClient = InfluxDBClientFactory.Create(influxOptions);

            services.AddSyntheticsInfluxAlerting(provider => influxClient.GetWriteApiAsync());
        }

        private static void ConfigureHttpTest(IServiceCollection services, HttpSyntheticTestConfiguration configuration)
        {
            services.AddSingleton<ISyntheticTest>(x => new HttpSyntheticTest(x.GetRequiredService<IHttpClientFactory>(), configuration));
        }
    }
}

