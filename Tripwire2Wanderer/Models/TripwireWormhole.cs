using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class TripwireWormhole
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("initialID")]
	public string InitialId { get; set; } = string.Empty;

	[JsonPropertyName("secondaryID")]
	public string SecondaryId { get; set; } = string.Empty;

	[JsonPropertyName("type")]
	public string SigType { get; set; } = string.Empty;

	[JsonPropertyName("parent")]
	public string Parent { get; set; } = string.Empty;

	[JsonPropertyName("life")]
	public string Life { get; set; } = string.Empty;

	[JsonPropertyName("mass")]
	public string Mass { get; set; } = string.Empty;

	[JsonPropertyName("maskID")]
	public string MaskId { get; set; } = string.Empty;
}
