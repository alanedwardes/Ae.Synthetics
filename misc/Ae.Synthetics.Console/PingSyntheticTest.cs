using System.Threading.Tasks;
using System;
using Ae.Synthetics.Runner;
using System.Threading;
using Microsoft.Extensions.Logging;
using Ae.Synthetics.Console.Configuration;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Ae.Synthetics.Console
{
    public sealed class PingSyntheticTest : ISyntheticTest
    {
        private sealed class PingSyntheticTestException : Exception
        {
            public PingSyntheticTestException(string message) : base(message)
            {
            }
        }

        private readonly PingSyntheticTestConfiguration _configuration;

        public string Name => _configuration.Name;

        public PingSyntheticTest(PingSyntheticTestConfiguration configuration) => _configuration = configuration;

        public async Task Run(ILogger logger, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            logger.LogInformation("Sending ping to {Host}", _configuration.Host);

            var ping = new Ping();

            var reply = await ping.SendPingAsync(_configuration.Host);
            if (reply.Status != IPStatus.Success)
            {
                throw new PingSyntheticTestException($"Got reply {reply.Status} from {reply.Address}");
            }

            logger.LogInformation("Completed ping to {Host} in {ElapsedMilliseconds}ms", _configuration.Host, sw.ElapsedMilliseconds);
        }
    }
}