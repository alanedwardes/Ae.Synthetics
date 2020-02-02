using Ae.Synthetics.Runner.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Ae.Synthetics.Runner
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSyntheticsRunner(this IServiceCollection services, SyntheticsRunnerConfig config)
        {
            return services.AddSingleton(config).AddSingleton<ISyntheticsRunner, SyntheticsRunner>();
        }
    }
}
