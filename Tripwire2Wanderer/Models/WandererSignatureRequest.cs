using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererSignatureRequest
{
    [JsonPropertyName("character_eve_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? CharacterEveId { get; set; }

    [JsonPropertyName("eve_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? EveId { get; set; }

    [JsonPropertyName("group")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Group { get; set; }

    [JsonPropertyName("kind")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Kind { get; set; }

    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Type { get; set; }

    [JsonPropertyName("solar_system_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int SolarSystemId { get; set; }

    [JsonPropertyName("linked_system_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? LinkedSystemId { get; set; }

    private void Reset()
    {
        CharacterEveId = null;
        EveId = null;
        Group = null;
        Kind = null;
        Name = null;
        Type = null;
        SolarSystemId = 0;
        LinkedSystemId = null;
    }

    public void CreateRequest(string? eveId, string? name, string eveCharId, string twType, string solarSystemId,
        string? linkedSystemId)
    {
        Reset();

        Name = name;
        Kind = "Cosmic Signature";
        CharacterEveId = eveCharId;

        twType = twType.ToLowerInvariant();

        switch (twType)
        {
            case "gas":
                Group = "Gas Site";
                break;
            case "data":
                Group = "Data Site";
                break;
            case "relic":
                Group = "Relic Site";
                break;
            case "wormhole":
                Group = "Wormhole";
                break;
            case "unknown":
                Name = "Unknown";
                Group = "Cosmic Signature";
                break;
            default:
                break;
        }

        EveId = ParseEveId(eveId);
        if (int.TryParse(solarSystemId, out var solarSystemIdInt))
        {
            SolarSystemId = solarSystemIdInt;
        }

        if (linkedSystemId is not null && int.TryParse(linkedSystemId, out var linkedId))
        {
            LinkedSystemId = linkedId;
        }
    }

    private string? ParseEveId(string? eveId)
    {
        if (string.IsNullOrEmpty(eveId) || eveId == "???")
        {
            return null;
        }

        // Check if eveId matches pattern [A-Z]{3}-\d{3}
        if (AppRegex.EveIdRegex().IsMatch(eveId))
        {
            return eveId;
        }

        if (AppRegex.TripWireSignatureIdRegex().IsMatch(eveId))
        {
            // Convert tripwire pattern abc123 to EVE pattern ABC-123
            var upper = eveId.ToUpper();
            return $"{upper[..3]}-{upper.Substring(3, 3)}";
        }

        return null;
    }
}