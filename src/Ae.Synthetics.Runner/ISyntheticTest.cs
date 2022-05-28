using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Synthetics.Runner
{
    public interface ISyntheticTest
    {
        string Name { get; }
        Task Run(ILogger logger, CancellationToken token);
    }
}
