using System;

namespace Ae.Synthetics.Console.Configuration
{
    public sealed class SyntheticsRunnerConfig
    {
        public bool RunChecksInParallel { get; set; } = true;
        public TimeSpan SyntheticTestTimeout { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan SyntheticTestCancellationGracePeriod { get; set; } = TimeSpan.FromSeconds(5);
    }
}