using InfluxDB.Client;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ae.Synthetics.Alerting.InfluxDb
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSyntheticsInfluxAlerting(this IServiceCollection services, Func<IServiceProvider, WriteApiAsync> config)
        {
            return services.AddSingleton<ISyntheticsAlerter>(provider => new InfluxSyntheticsAlerter(config(provider)));
        }
    }
}
