using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererSignatureEnvelope
{
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<WandererSignature> Data { get; set; } = new();
}