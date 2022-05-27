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
                throw new InvalidOperationException($"No alerters found - register alerters against the interface {nameof(ISyntheticsAlerter)}.");
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
            // Construct a cancellation source for the delay which will prevent tests hanging forever
            using var watchDogInnerCancellationSource = new CancellationTokenSource(_config.SyntheticTestTimeout + _config.SyntheticTestCancellationGracePeriod);
            using var watchDogCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(watchDogInnerCancellationSource.Token, token);

            // Construct a cancellation source for the test itself
            using var syntheticTestInnerCancellationSource = new CancellationTokenSource(_config.SyntheticTestTimeout);
            using var syntheticTestCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(syntheticTestInnerCancellationSource.Token, token);

            var testLogger = new SyntheticTestLogger(_config.LogFormatter);

            var sw = Stopwatch.StartNew();
            try
            {
                // Wait forever with the cancellation source which includes a grace period
                using var watchDogTask = Task.Delay(-1, watchDogCancellationSource.Token);

                // Run the task on a new thread with cancellation (don't dispose, this becomes too complex)
                var testTask = Task.Run(() => syntheticTest.Run(testLogger, syntheticTestCancellationSource.Token));

                // Wait for either the watchdog to finish/cancel, or the test to finish/cancel
                await Task.WhenAny(testTask, watchDogTask);

                // If the watchdog kicked in and the task is still going, it is hung
                if (watchDogTask.IsCanceled && testTask.Status != TaskStatus.RanToCompletion)
                {
                    throw new TaskCanceledException($"Synthetic test was abandoned because it did not respond to cancellation. It was left running, and may still be consuming resources.");
                }

                // Stop the watchdog otherwise it cannot be disposed
                watchDogCancellationSource.Cancel();

                // Allow exceptions to propagate
                await testTask;

                await Task.WhenAll(alerters.Select(x => x.Success(testLogger.Entries, syntheticTest.GetType(), sw.Elapsed, token)));
            }
            catch (Exception e)
            {
                await Task.WhenAll(alerters.Select(x => x.Failure(testLogger.Entries, syntheticTest.GetType(), e, sw.Elapsed, token)));
            }
        }
    }
}
