using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ae.Synthetics.Alerting
{
    public sealed class SyntheticsLoggerAlerter : ISyntheticsAlerter
    {
        private readonly ILogger<SyntheticsLoggerAlerter> _logger;

        public SyntheticsLoggerAlerter(ILogger<SyntheticsLoggerAlerter> logger) => _logger = logger;

        public Task Failure(IReadOnlyList<string> logEntries, string source, Exception exception, TimeSpan time, CancellationToken token)
        {
            _logger.LogError(exception, string.Join(Environment.NewLine, logEntries));
            return Task.CompletedTask;
        }

        public Task Success(IReadOnlyList<string> logEntries, string source, TimeSpan time, CancellationToken token)
        {
            _logger.LogInformation(string.Join(Environment.NewLine, logEntries));
            return Task.CompletedTask;
        }
    }
}
