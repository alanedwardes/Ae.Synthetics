using Microsoft.Extensions.Logging;
using System;

namespace Ae.Synthetics.Runner
{
    public sealed class SyntheticsRunnerConfig
    {
        public TimeSpan SyntheticTestTimeout { get; set; }
        public Func<DateTimeOffset, LogLevel, EventId, string, string> LogFormatter { get; set; } = (dt, level, id, message) => $"[{level.ToString().ToUpper()} {dt:O}] {message}";
    }
}
