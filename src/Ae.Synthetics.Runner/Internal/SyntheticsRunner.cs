using Ae.Synthetics.Alerting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Synthetics.Runner.Internal
{
    internal sealed class SyntheticsRunner : ISyntheticsRunner
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SyntheticsRunnerConfig _config;

        public SyntheticsRunner(IServiceProvider serviceProvider, SyntheticsRunnerConfig config)
        {
            _serviceProvider = serviceProvider;
            _config = config;
        }

        public async Task RunSyntheticTests(CancellationToken token)
        {
            var syntheticTests = _serviceProvider.GetServices<ISyntheticTest>().ToArray();
            var alerters = _serviceProvider.GetServices<ISyntheticsAlerter>().ToArray();

            if (syntheticTests.Length == 0)
            {
                throw new InvalidOperationException($"No tests found - register tests against the interface {nameof(ISyntheticTest)}.");
            }

            if (alerters.Length == 0)
            {
                throw new InvalidOperationException($"No tests found - register alerters against the interface {nameof(ISyntheticsAlerter)}.");
            }

            await Task.WhenAll(syntheticTests.Select(x => RunSyntheticTest(x, alerters, token)));
        }

        public async Task RunSyntheticTestsForever(TimeSpan frequency, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await RunSyntheticTests(token);
                await Task.Delay(frequency, token);
            }
        }

        private async Task RunSyntheticTest(ISyntheticTest syntheticTest, ISyntheticsAlerter[] alerters, CancellationToken token)
        {
            var timeoutSource = new CancellationTokenSource(_config.SyntheticTestTimeout);
            var syntheticTestSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, token);

            var testLogger = new SyntheticTestLogger(_config.LogFormatter);

            var sw = Stopwatch.StartNew();
            try
            {
                await syntheticTest.Run(testLogger, syntheticTestSource.Token);
            }
            catch (Exception e)
            {
                await Task.WhenAll(alerters.Select(x => x.AlertFailure(testLogger.Entries, syntheticTest.GetType(), e, sw.Elapsed, token)));
            }
            finally
            {
                // TODO: publish metrics
            }
        }
    }
}
