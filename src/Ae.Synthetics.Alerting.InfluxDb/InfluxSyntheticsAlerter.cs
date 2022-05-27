using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Synthetics.Alerting.Ses
{
    internal sealed class InfluxSyntheticsAlerter : ISyntheticsAlerter
    {
        private readonly WriteApiAsync _write;

        public InfluxSyntheticsAlerter(WriteApiAsync write) => _write = write;

        public Task Failure(IReadOnlyList<string> logEntries, Type source, Exception exception, TimeSpan time, CancellationToken token)
        {
            return WritePoint(source, time, false, token);
        }

        public Task Success(IReadOnlyList<string> logEntries, Type source, TimeSpan time, CancellationToken token)
        {
            return WritePoint(source, time, true, token);
        }

        private Task WritePoint(Type source, TimeSpan time, bool success, CancellationToken token)
        {
            var point = PointData.Measurement("synthetic")
                        .Tag("source", source.Name)
                        .Field("latency", time)
                        .Field("success", success)
                        .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            return _write.WritePointAsync(point, null, null, token);
        }
    }
}
