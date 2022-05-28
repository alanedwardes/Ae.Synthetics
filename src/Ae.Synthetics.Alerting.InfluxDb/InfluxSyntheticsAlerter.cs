using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Synthetics.Alerting.InfluxDb
{
    internal sealed class InfluxSyntheticsAlerter : ISyntheticsAlerter
    {
        private readonly WriteApiAsync _write;

        public InfluxSyntheticsAlerter(WriteApiAsync write) => _write = write;

        public Task Failure(IReadOnlyList<string> logEntries, string source, Exception exception, TimeSpan time, CancellationToken token)
        {
            return WritePoint(source, time, false, token);
        }

        public Task Success(IReadOnlyList<string> logEntries, string source, TimeSpan time, CancellationToken token)
        {
            return WritePoint(source, time, true, token);
        }

        private Task WritePoint(string source, TimeSpan time, bool success, CancellationToken token)
        {
            var point = PointData.Measurement("synthetic")
                        .Tag("source", source)
                        .Field("latency", time.TotalMilliseconds)
                        .Field("success", success ? 1 : 0)
                        .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            return _write.WritePointAsync(point, null, null, token);
        }
    }
}
