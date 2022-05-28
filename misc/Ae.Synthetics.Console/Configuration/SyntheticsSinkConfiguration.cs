using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ae.Synthetics.Console.Configuration
{
    public sealed class SyntheticsSinkConfiguration
    {
		public string Type { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
		public JsonElement Configuration { get; set; } = new JsonElement();
	}
}

