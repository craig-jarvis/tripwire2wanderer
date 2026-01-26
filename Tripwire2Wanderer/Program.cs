using System.Text.Json;
using Tripwire2Wanderer;
using Tripwire2Wanderer.Clients;

try
{
	// Load configuration from .env and environment variables
	var config = Config.Load();

	// Create Tripwire API client
	var twClient = new TripwireClient(config);

	// Create Wanderer API client
	var wClient = new WandererClient(config);

	// Fetch wormholes
	var wormholes = await twClient.GetWormholesAsync();

	// Fetch signatures
	var signatures = await twClient.GetSignaturesAsync();

	// Output counts
	Console.WriteLine($"Wormholes: {wormholes.Count}");
	Console.WriteLine($"Signatures: {signatures.Count}");

	// Get current systems and connections from Wanderer
	Console.WriteLine("\n--- Fetching current Wanderer data ---");
	var wandererData = await wClient.GetSystemsAndConnectionsAsync();

	// Build the map recursively from home system
	var mapResult = DataHelpers.BuildWandererMapRecursive(config.WandererHomeSystemId, signatures, wormholes);
	Console.WriteLine($"wanderer request {mapResult.Data.Connections.Count} connections and {mapResult.Data.Systems.Count} systems");

	mapResult = DataHelpers.DedupWandererEnvelope(mapResult);
	mapResult = DataHelpers.CalculateSystemPositions(
			mapResult,
			config.WandererHomeSystemId,
			config.PositionXSeparation,
			config.PositionYSeparation);

	var mapResultJson = JsonSerializer.Serialize(mapResult, new JsonSerializerOptions { WriteIndented = true });
	Console.WriteLine(mapResultJson);

	// Compare and prepare delete request
	var deleteRequest = DataHelpers.CompareWandererEnvelopes(wandererData, mapResult);

	// Delete no longer on map systems and connections
	Console.WriteLine("\n--- Deleting old Wanderer data ---");
	await wClient.DeleteSystemsAndConnectionsAsync(deleteRequest);

	// Post current systems and connections for Wanderer
	Console.WriteLine("\n--- Adding/updating new sigs and connections ---");
	var wandererResponse = await wClient.SubmitConnectionsAndSystemsAsync(mapResult);

	var wandererJson = JsonSerializer.Serialize(wandererResponse, new JsonSerializerOptions { WriteIndented = true });
	Console.WriteLine(wandererJson);
}
catch (Exception ex)
{
	Console.WriteLine($"Error: {ex.Message}");
	Environment.Exit(1);
}
