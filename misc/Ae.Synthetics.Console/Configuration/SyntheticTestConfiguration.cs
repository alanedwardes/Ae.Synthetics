using System.Text.Json;

namespace Ae.Synthetics.Console.Configuration
{
    public sealed class SyntheticTestConfiguration
    {
		public string Type { get; set; }
		public JsonElement Configuration { get; set; } = new JsonElement();
	}
}

