using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererConnection
{
	[JsonPropertyName("custom_info")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string CustomInfo { get; set; } = string.Empty;

	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("locked")]
	public bool Locked { get; set; }

	[JsonPropertyName("map_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string MapId { get; set; } = string.Empty;

	[JsonPropertyName("mass_status")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int MassStatus { get; set; }

	[JsonPropertyName("ship_size_type")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int ShipSizeType { get; set; }

	[JsonPropertyName("solar_system_source")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int SolarSystemSource { get; set; }

	[JsonPropertyName("solar_system_target")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int SolarSystemTarget { get; set; }

	[JsonPropertyName("time_status")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int TimeStatus { get; set; }

	[JsonPropertyName("type")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int Type { get; set; }

	[JsonPropertyName("wormhole_type")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string WormholeType { get; set; } = string.Empty;
}
