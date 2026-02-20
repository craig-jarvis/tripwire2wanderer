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

    public static string MapTripwireTypeToWandererGroup(string? tripwireType)
    {
        return tripwireType?.ToLowerInvariant() switch
        {
            "combat" => "Combat Site",
            "data" => "Data Site",
            "gas" => "Gas Site",
            "ore" => "Ore Site",
            "relic" => "Relic Site",
            "wormhole" => "Wormhole",
            _ => "unknown"
        };
    }

    public static WandererSignatureRequest FromTripwireSignature(
        TripwireSignature twSignature,
        List<TripwireWormhole> wormholes,
        List<TripwireSignature> allSignatures,
        string characterEveId)
    {
        var signatureGroup = MapTripwireTypeToWandererGroup(twSignature.Type);

        var request = new WandererSignatureRequest
        {
            CharacterEveId = characterEveId,
            EveId = ParseEveId(twSignature.SignatureId),
            SolarSystemId = int.TryParse(twSignature.SystemId, out var sysId) ? sysId : 0,
            Group = signatureGroup
        };

        // If this is a wormhole signature, find the linked system
        if (signatureGroup == "Wormhole")
        {
            // Find the wormhole connection where this signature is either initial or secondary
            var wormhole = wormholes.FirstOrDefault(wh =>
                wh.InitialId == twSignature.Id || wh.SecondaryId == twSignature.Id);

            if (wormhole != null)
            {
                // Find the signature on the other side
                var otherSigId = wormhole.InitialId == twSignature.Id
                    ? wormhole.SecondaryId
                    : wormhole.InitialId;

                var otherSignature = allSignatures.FirstOrDefault(s => s.Id == otherSigId);

                // If we have the other signature and it has a valid system ID, use it
                if (otherSignature != null && int.TryParse(otherSignature.SystemId, out var linkedSysId))
                    request.LinkedSystemId = linkedSysId;
            }
        }

        return request;
    }

    private static string? ParseEveId(string? eveId)
    {
        if (string.IsNullOrEmpty(eveId) || eveId == "???") return null;

        // Check if eveId matches pattern [A-Z]{3}-\d{3}
        if (AppRegex.EveIdRegex().IsMatch(eveId)) return eveId;

        if (AppRegex.TripWireSignatureIdRegex().IsMatch(eveId))
        {
            // Convert tripwire pattern abc123 to EVE pattern ABC-123
            var upper = eveId.ToUpper();
            return $"{upper[..3]}-{upper.Substring(3, 3)}";
        }

        return null;
    }
}