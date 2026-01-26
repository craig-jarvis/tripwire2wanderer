using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererConnectionAndSystemResponse
{
	[JsonPropertyName("updated")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int Updated { get; set; }

	[JsonPropertyName("created")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int Created { get; set; }
}
