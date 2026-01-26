using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererConnectionAndSystemCreateResponseEnvelope
{
	[JsonPropertyName("data")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public WandererConnectionAndSystemCreateResponse Data { get; set; } = new();
}
