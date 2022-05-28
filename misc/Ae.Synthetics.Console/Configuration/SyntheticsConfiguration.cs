using System;
using System.Collections.Generic;

namespace Ae.Synthetics.Console.Configuration
{
    public sealed class SyntheticsConfiguration
	{
		public TimeSpan Interval { get; set; }
		public IList<SyntheticsSinkConfiguration> Sinks { get; set; } = Array.Empty<SyntheticsSinkConfiguration>();
		public IList<SyntheticTestConfiguration> Tests { get; set; } = Array.Empty<SyntheticTestConfiguration>();
	}
}

