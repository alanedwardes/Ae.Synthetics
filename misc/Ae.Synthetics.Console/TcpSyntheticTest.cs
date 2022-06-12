using System.Threading.Tasks;
using Ae.Synthetics.Runner;
using System.Threading;
using Microsoft.Extensions.Logging;
using Ae.Synthetics.Console.Configuration;
using System.Net.Sockets;
using System.Diagnostics;

namespace Ae.Synthetics.Console
{
    public sealed class TcpSyntheticTest : ISyntheticTest
    {
        private readonly TcpSyntheticTestConfiguration _configuration;

        public string Name => _configuration.Name;

        public TcpSyntheticTest(TcpSyntheticTestConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Run(ILogger logger, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            logger.LogInformation("Making TCP connection to {Host}:{Port}", _configuration.Host, _configuration.Port);

            using (var socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
            {
                await socket.ConnectAsync(_configuration.Host, _configuration.Port, token);

                logger.LogInformation("Opened TCP connection to {Host}:{Port} in {ElapsedMilliseconds}ms", _configuration.Host, _configuration.Port, sw.ElapsedMilliseconds);

                await socket.DisconnectAsync(false, token);
            }

            logger.LogInformation("Closed TCP connection to {Host}:{Port} in {ElapsedMilliseconds}ms", _configuration.Host, _configuration.Port, sw.ElapsedMilliseconds);
        }
    }
}