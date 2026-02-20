using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererConnectionsAndSystemsEnvelope
{
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public WandererConnectionsAndSystems Data { get; set; } = new();
}