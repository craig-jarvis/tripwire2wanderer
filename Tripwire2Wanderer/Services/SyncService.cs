using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Tripwire2Wanderer.Clients;

namespace Tripwire2Wanderer.Services;

public class SyncService : BackgroundService
{
	private readonly Config _config;
	private readonly TripwireClient _tripwireClient;
	private readonly WandererClient _wandererClient;
	private readonly bool _debug;

	public SyncService(Config config, TripwireClient tripwireClient, WandererClient wandererClient)
	{
		_config = config;
		_tripwireClient = tripwireClient;
		_wandererClient = wandererClient;
		
		// Check for debug mode from environment or command line
		_debug = Environment.GetCommandLineArgs().Contains("--debug") || 
		         Environment.GetEnvironmentVariable("DEBUG") == "true";
		
		if (_debug)
		{
			Console.WriteLine("Running in DEBUG mode - Wanderer API calls will be skipped");
		}
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    Console.WriteLine($"Poll interval: {_config.PollIntervalSeconds} seconds");
    Console.WriteLine("Starting sync loop...\n");

    while (!stoppingToken.IsCancellationRequested)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            await RunSyncAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Sync cancelled due to shutdown");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during sync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        var elapsed = DateTime.UtcNow - startTime;
        Console.WriteLine($"\nSync completed in {elapsed.TotalSeconds:F2} seconds");

        // Always wait for the full configured interval before next sync
        Console.WriteLine($"Waiting {_config.PollIntervalSeconds} seconds until next sync...\n");
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(_config.PollIntervalSeconds), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Wait cancelled due to shutdown");
            break;
        }
    }

    Console.WriteLine("Shutdown complete");
}

	private async Task RunSyncAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Starting sync...");

		// Fetch wormholes
		var wormholes = await _tripwireClient.GetWormholesAsync(cancellationToken);

		// Fetch signatures
		var signatures = await _tripwireClient.GetSignaturesAsync(cancellationToken);

		// Output counts
		Console.WriteLine($"Wormholes: {wormholes.Count}");
		Console.WriteLine($"Signatures: {signatures.Count}");

		// Build the map recursively from home system
		var mapResult = DataHelpers.BuildWandererMapRecursive(_config.WandererHomeSystemId, signatures, wormholes);
		Console.WriteLine(
			$"Built map with {mapResult.Data.Connections.Count} connections and {mapResult.Data.Systems.Count} systems");

		mapResult = DataHelpers.DedupWandererEnvelope(mapResult);
		mapResult = DataHelpers.CalculateSystemPositions(
			mapResult,
			_config.WandererHomeSystemId,
			_config.PositionXSeparation,
			_config.PositionYSeparation);

		if (!_debug)
		{
			// Get current systems and connections from Wanderer
			Console.WriteLine("\n--- Fetching current Wanderer data ---");
			var wandererData = await _wandererClient.GetSystemsAndConnectionsAsync(cancellationToken);

			// Check if there are any changes needed
			if (!DataHelpers.HasChanges(wandererData, mapResult))
			{
				Console.WriteLine("No changes detected - skipping Wanderer update");
				return;
			}

			Console.WriteLine("Changes detected - syncing to Wanderer");

			// Compare and prepare delete request
			var deleteRequest = DataHelpers.CompareWandererEnvelopes(wandererData, mapResult);

			// Delete no longer on map systems and connections
			Console.WriteLine($"--- Deleting {deleteRequest.ConnectionIds.Count} connections and {deleteRequest.SystemIds.Count} systems from Wanderer ---");
			await _wandererClient.DeleteSystemsAndConnectionsAsync(deleteRequest, cancellationToken);

			// Post current systems and connections for Wanderer
			var wandererResponse = await _wandererClient.SubmitConnectionsAndSystemsAsync(mapResult, cancellationToken);
			Console.WriteLine($"--- Added {wandererResponse.Data.Systems.Created} / Updated {wandererResponse.Data.Systems.Updated} systems ---");
			Console.WriteLine($"--- Added {wandererResponse.Data.Connections.Created} / Updated {wandererResponse.Data.Connections.Updated} connections ---");
		}
		else
		{
			Console.WriteLine("\n--- DEBUG MODE: Skipping Wanderer API calls ---");
			var mapResultJson = JsonSerializer.Serialize(mapResult, new JsonSerializerOptions { WriteIndented = true });
			Console.WriteLine(mapResultJson);
		}
	}
}
