using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererSignature
{
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Name { get; set; }

	[JsonPropertyName("type")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Type { get; set; }

	[JsonPropertyName("description")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Description { get; set; }

	[JsonPropertyName("group")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Group { get; set; }

	[JsonPropertyName("kind")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Kind { get; set; }

	[JsonPropertyName("deleted")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool Deleted { get; set; }

	[JsonPropertyName("inserted_at")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? InsertedAt { get; set; }

	[JsonPropertyName("updated_at")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? UpdatedAt { get; set; }

	[JsonPropertyName("temporary_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? TemporaryName { get; set; }

	[JsonPropertyName("solar_system_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int SolarSystemId { get; set; }

	[JsonPropertyName("custom_info")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? CustomInfo { get; set; }

	[JsonPropertyName("linked_system_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int? LinkedSystemId { get; set; }

	[JsonPropertyName("eve_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? EveId { get; set; }

	[JsonPropertyName("character_eve_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? CharacterEveId { get; set; }

	[JsonPropertyName("update_forced_at")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? UpdateForcedAt { get; set; }
}
