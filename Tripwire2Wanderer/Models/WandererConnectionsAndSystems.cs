using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererConnectionsAndSystems
{
    [JsonPropertyName("connections")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<WandererConnection> Connections { get; set; } = new();

    [JsonPropertyName("systems")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<WandererSystem> Systems { get; set; } = new();
}