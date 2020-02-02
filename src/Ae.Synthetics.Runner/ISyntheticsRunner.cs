using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Synthetics.Runner
{
    public interface ISyntheticsRunner
    {
        Task RunSyntheticTestsForever(TimeSpan frequency, CancellationToken token);
        Task RunSyntheticTests(CancellationToken token);
    }
}
