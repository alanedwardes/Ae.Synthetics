using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Synthetics.Alerting
{
    public interface ISyntheticsAlerter
    {
        Task Failure(IReadOnlyList<string> logEntries, Type source, Exception exception, TimeSpan time, CancellationToken token);
        Task Success(IReadOnlyList<string> logEntries, Type source, TimeSpan time, CancellationToken token);
    }
}
