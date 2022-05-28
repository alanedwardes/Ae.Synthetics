using Microsoft.Extensions.Logging;
using System;

namespace Ae.Synthetics.Runner
{
    public sealed class SyntheticsRunnerConfig
    {
        /// <summary>
        /// If true, run the checks at the same time, if false sequentially.
        /// </summary>
        public bool RunChecksInParallel { get; set; } = true;
        /// <summary>
        /// The amount of time each test is given to run to completion.
        /// </summary>
        public TimeSpan SyntheticTestTimeout { get; set; } = TimeSpan.FromSeconds(15);
        /// <summary>
        /// The amount of time each test is given to cancel, once the timeout expires or the runner is cancelled.
        /// </summary>
        public TimeSpan SyntheticTestCancellationGracePeriod { get; set; } = TimeSpan.FromSeconds(5);
        /// <summary>
        /// The formatter to use when formatting log messages from the alerter.
        /// </summary>
        public Func<DateTimeOffset, LogLevel, EventId, string, string> LogFormatter { get; set; } = (dt, level, id, message) => $"[{level.ToString().ToUpper()} {dt:O}] {message}";
    }
}
