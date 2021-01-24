using Ae.Synthetics.Alerting;
using Ae.Synthetics.Runner;
using Ae.Synthetics.Runner.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ae.Synthetics.Tests
{
    public class SyntheticsRunnerTests
    {
        public class TestRunner : ISyntheticTest
        {
            public bool DidRun { get; private set; }

            public Task Run(ILogger logger, CancellationToken token)
            {
                DidRun = true;
                return Task.CompletedTask;
            }
        }

        public class TestAlerter : ISyntheticsAlerter
        {
            public bool DidAlert { get; private set; }

            public Task AlertFailure(IReadOnlyList<string> logEntries, Type source, Exception exception, TimeSpan time, CancellationToken token)
            {
                DidAlert = true;
                return Task.CompletedTask;
            }
        }

        public class TestHangingSynchronousRunner : ISyntheticTest
        {
            public Task Run(ILogger logger, CancellationToken token)
            {
                while (true)
                {
                }
            }
        }

        public class TestCancellableAsynchronousRunner : ISyntheticTest
        {
            public async Task Run(ILogger logger, CancellationToken token) => await Task.Delay(-1, token);
        }

        public class TestHangingAsynchronousRunner : ISyntheticTest
        {
            public async Task Run(ILogger logger, CancellationToken token) => await Task.Delay(-1);
        }

        [Fact]
        public async Task TestSyntheticsRunner()
        {
            var testRunner = new TestRunner();
            var testAlerter = new TestAlerter();

            var provider = new ServiceCollection()
                .AddSingleton<ISyntheticTest>(testRunner)
                .AddSingleton<ISyntheticsAlerter>(testAlerter)
                .BuildServiceProvider();

            var runner = new SyntheticsRunner(provider, new SyntheticsRunnerConfig());

            await runner.RunSyntheticTests(CancellationToken.None);

            Assert.True(testRunner.DidRun);
            Assert.False(testAlerter.DidAlert);
        }

        [Fact]
        public async Task TestSyntheticsRunnerKillHangingSynchronousRunner()
        {
            var testAlerter = new TestAlerter();

            var provider = new ServiceCollection()
                .AddSingleton<ISyntheticTest>(new TestHangingSynchronousRunner())
                .AddSingleton<ISyntheticsAlerter>(testAlerter)
                .BuildServiceProvider();

            var runner = new SyntheticsRunner(provider, new SyntheticsRunnerConfig
            {
                SyntheticTestTimeout = TimeSpan.FromMilliseconds(500),
                SyntheticTestCancellationGracePeriod = TimeSpan.FromMilliseconds(100)
            });

            await runner.RunSyntheticTests(CancellationToken.None);

            Assert.True(testAlerter.DidAlert);
        }

        [Fact]
        public async Task TestSyntheticsRunnerCancelAsynchronousRunner()
        {
            var testAlerter = new TestAlerter();

            var provider = new ServiceCollection()
                .AddSingleton<ISyntheticTest>(new TestCancellableAsynchronousRunner())
                .AddSingleton<ISyntheticsAlerter>(testAlerter)
                .BuildServiceProvider();

            var runner = new SyntheticsRunner(provider, new SyntheticsRunnerConfig
            {
                SyntheticTestTimeout = TimeSpan.FromMilliseconds(500)
            });

            await runner.RunSyntheticTests(CancellationToken.None);

            Assert.True(testAlerter.DidAlert);
        }

        [Fact]
        public async Task TestSyntheticsRunnerKillHangingAsynchronousRunner()
        {
            var testAlerter = new TestAlerter();

            var provider = new ServiceCollection()
                .AddSingleton<ISyntheticTest>(new TestHangingAsynchronousRunner())
                .AddSingleton<ISyntheticsAlerter>(testAlerter)
                .BuildServiceProvider();

            var runner = new SyntheticsRunner(provider, new SyntheticsRunnerConfig
            {
                SyntheticTestTimeout = TimeSpan.FromMilliseconds(500),
                SyntheticTestCancellationGracePeriod = TimeSpan.FromMilliseconds(100)
            });

            await runner.RunSyntheticTests(CancellationToken.None);

            Assert.True(testAlerter.DidAlert);
        }

        [Fact]
        public async Task TestSyntheticsRunnerCancellationToken()
        {
            var testAlerter = new TestAlerter();

            var provider = new ServiceCollection()
                .AddSingleton<ISyntheticTest>(new TestCancellableAsynchronousRunner())
                .AddSingleton<ISyntheticTest>(new TestHangingSynchronousRunner())
                .AddSingleton<ISyntheticsAlerter>(testAlerter)
                .BuildServiceProvider();

            var runner = new SyntheticsRunner(provider, new SyntheticsRunnerConfig());

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

            await runner.RunSyntheticTests(cancellationTokenSource.Token);

            Assert.True(testAlerter.DidAlert);
        }
    }
}
