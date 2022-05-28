using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Synthetics.Alerting
{
    public interface ISyntheticsAlerter
    {
        Task Failure(IReadOnlyList<string> logEntries, string source, Exception exception, TimeSpan time, CancellationToken token);
        Task Success(IReadOnlyList<string> logEntries, string source, TimeSpan time, CancellationToken token);
    }
}
