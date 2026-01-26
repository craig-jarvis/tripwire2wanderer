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

public class WandererSystem
{
	[JsonPropertyName("id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("status")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int Status { get; set; }

	[JsonPropertyName("tag")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Tag { get; set; }

	[JsonPropertyName("visible")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public bool Visible { get; set; }

	[JsonPropertyName("description")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Description { get; set; }

	[JsonPropertyName("labels")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Labels { get; set; }

	[JsonPropertyName("inserted_at")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string InsertedAt { get; set; } = string.Empty;

	[JsonPropertyName("updated_at")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string UpdatedAt { get; set; } = string.Empty;

	[JsonPropertyName("locked")]
	public bool Locked { get; set; }

	[JsonPropertyName("map_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string MapId { get; set; } = string.Empty;

	[JsonPropertyName("temporary_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? TemporaryName { get; set; }

	[JsonPropertyName("solar_system_id")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int SolarSystemId { get; set; }

	[JsonPropertyName("position_y")]
	public double PositionY { get; set; }

	[JsonPropertyName("position_x")]
	public double PositionX { get; set; }

	[JsonPropertyName("custom_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? CustomName { get; set; }

	[JsonPropertyName("original_name")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string OriginalName { get; set; } = string.Empty;
}

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
	public string Group { get; set; } = string.Empty;

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
	public string Name { get; set; } = string.Empty;

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

public class WandererConnectionsAndSystems
{
	[JsonPropertyName("connections")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public List<WandererConnection> Connections { get; set; } = new();

	[JsonPropertyName("systems")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public List<WandererSystem> Systems { get; set; } = new();
}

public class WandererConnectionsAndSystemsEnvelope
{
	[JsonPropertyName("data")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public WandererConnectionsAndSystems Data { get; set; } = new();
}

public class WandererConnectionAndSystemResponse
{
	[JsonPropertyName("updated")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int Updated { get; set; }

	[JsonPropertyName("created")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public int Created { get; set; }
}

public class WandererConnectionAndSystemCreateResponse
{
	[JsonPropertyName("connections")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public WandererConnectionAndSystemResponse Connections { get; set; } = new();

	[JsonPropertyName("systems")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public WandererConnectionAndSystemResponse Systems { get; set; } = new();
}

public class WandererConnectionAndSystemCreateResponseEnvelope
{
	[JsonPropertyName("data")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public WandererConnectionAndSystemCreateResponse Data { get; set; } = new();
}

public class WandererSystemAndConnectionsDeleteRequest
{
	[JsonPropertyName("connection_ids")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public List<string> ConnectionIds { get; set; } = new();

	[JsonPropertyName("system_ids")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public List<int> SystemIds { get; set; } = new();
}
