using System.Text.Json.Serialization;

namespace Tripwire2Wanderer.Models;

public class TripwireSignature
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("signatureID")]
	public string? SignatureId { get; set; } = string.Empty;

	[JsonPropertyName("systemID")]
	public string? SystemId { get; set; } = string.Empty;

	[JsonPropertyName("type")]
	public string? Type { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	public string? Name { get; set; } = string.Empty;

	[JsonPropertyName("bookmark")]
	public string? Bookmark { get; set; }

	[JsonPropertyName("lifeTime")]
	public string LifeTime { get; set; } = string.Empty;

	[JsonPropertyName("lifeLeft")]
	public string LifeLeft { get; set; } = string.Empty;

	[JsonPropertyName("lifeLength")]
	public string LifeLength { get; set; } = string.Empty;

	[JsonPropertyName("createdByID")]
	public string CreatedById { get; set; } = string.Empty;

	[JsonPropertyName("createdByName")]
	public string CreatedByName { get; set; } = string.Empty;

	[JsonPropertyName("modifiedByID")]
	public string ModifiedById { get; set; } = string.Empty;

	[JsonPropertyName("modifiedByName")]
	public string ModifiedByName { get; set; } = string.Empty;

	[JsonPropertyName("modifiedTime")]
	public string ModifiedTime { get; set; } = string.Empty;

	[JsonPropertyName("maskID")]
	public string MaskId { get; set; } = string.Empty;
}
