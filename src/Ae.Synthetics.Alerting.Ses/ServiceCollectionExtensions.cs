using Amazon.SimpleEmail;
using Microsoft.Extensions.DependencyInjection;

namespace Ae.Synthetics.Alerting.Ses
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSyntheticsSesAlerting(this IServiceCollection services, SesSyntheticsAlerterConfig config)
        {
            return services.AddSingleton(config)
                           .AddSingleton<ISyntheticsAlerter, SesSyntheticsAlerter>()
                           .AddSingleton<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();
        }
    }
}
