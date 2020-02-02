using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Synthetics.Runner
{
    public interface ISyntheticTest
    {
        Task Run(ILogger logger, CancellationToken token);
    }
}
