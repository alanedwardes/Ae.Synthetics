using System;
using System.Collections.Generic;
using Ae.Synthetics.Runner;

namespace Ae.Synthetics.Console.Configuration
{
    public sealed class SyntheticsConfiguration
	{
		public TimeSpan Interval { get; set; }
		public SyntheticsRunnerConfig Runner { get; set; } = new SyntheticsRunnerConfig();
		public IList<SyntheticsSinkConfiguration> Sinks { get; set; } = Array.Empty<SyntheticsSinkConfiguration>();
		public IList<SyntheticTestConfiguration> Tests { get; set; } = Array.Empty<SyntheticTestConfiguration>();
	}
}

