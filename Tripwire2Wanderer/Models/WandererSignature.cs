using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class WandererSignature
{
	[JsonPropertyName("character_eve_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string CharacterEveId { get; set; } = string.Empty;

	[JsonPropertyName("custom_info")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string CustomInfo { get; set; } = string.Empty;

	[JsonPropertyName("description")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("eve_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string EveId { get; set; } = string.Empty;

	[JsonPropertyName("group")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Group { get; set; } = string.Empty;

	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("inserted_at")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public DateTime InsertedAt { get; set; }

	[JsonPropertyName("kind")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Kind { get; set; } = string.Empty;

	[JsonPropertyName("linked_system_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int LinkedSystemId { get; set; }

	[JsonPropertyName("name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Name { get; set; } = string.Empty;

	[JsonPropertyName("solar_system_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int SolarSystemId { get; set; }

	[JsonPropertyName("type")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Type { get; set; } = string.Empty;

	[JsonPropertyName("updated")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int Updated { get; set; }

	[JsonPropertyName("updated_at")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public DateTime UpdatedAt { get; set; }
}
