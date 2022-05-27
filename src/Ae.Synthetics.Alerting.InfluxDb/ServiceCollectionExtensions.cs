using InfluxDB.Client;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ae.Synthetics.Alerting.Ses
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSyntheticsInfluxAlerting(this IServiceCollection services, Func<IServiceProvider, WriteApiAsync> config)
        {
            return services.AddSingleton(provider => new InfluxSyntheticsAlerter(config(provider)));
        }
    }
}
