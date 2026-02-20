using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererSystemAndConnectionsDeleteRequest
{
    [JsonPropertyName("connection_ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<string> ConnectionIds { get; set; } = new();

    [JsonPropertyName("system_ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<int> SystemIds { get; set; } = new();
}