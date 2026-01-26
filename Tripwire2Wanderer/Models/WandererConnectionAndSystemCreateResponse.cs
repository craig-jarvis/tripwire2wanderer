using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererConnectionAndSystemCreateResponse
{
	[JsonPropertyName("connections")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public WandererConnectionAndSystemResponse Connections { get; set; } = new();

	[JsonPropertyName("systems")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public WandererConnectionAndSystemResponse Systems { get; set; } = new();
}
