using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Polly.Timeout;
using Tripwire2Wanderer.Clients;

namespace Tripwire2Wanderer.Services;

public class SyncService : BackgroundService
{
    private readonly Config _config;
    private readonly bool _debug;
    private readonly TripwireClient _tripwireClient;
    private readonly WandererClient _wandererClient;

    public SyncService(Config config, TripwireClient tripwireClient, WandererClient wandererClient)
    {
        _config = config;
        _tripwireClient = tripwireClient;
        _wandererClient = wandererClient;

        // Check for debug mode from environment or command line
        _debug = Environment.GetCommandLineArgs().Contains("--debug") ||
                 Environment.GetEnvironmentVariable("DEBUG") == "true";

        if (_debug) Console.WriteLine("Running in DEBUG mode - Wanderer API calls will be skipped");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Poll interval: {_config.PollIntervalSeconds} seconds");
        Console.WriteLine("Starting sync loop...\n");
        
        stoppingToken.Register(() =>
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Cancellation requested!"));

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
        try
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Starting sync...");

            // Fetch wormholes
            var wormholes = await _tripwireClient.GetWormholesAsync(cancellationToken);

            // Fetch signatures
            var signatures = await _tripwireClient.GetSignaturesAsync(cancellationToken);

            // Output counts
            Console.WriteLine($"Wormholes: {wormholes.Count}");
            Console.WriteLine($"Signatures: {signatures.Count}");

            // Get locked systems from Wanderer first (if not in debug mode)
            var lockedSystems = new Dictionary<int, bool>();
            if (!_debug)
            {
                var wandererData = await _wandererClient.GetSystemsAndConnectionsAsync(cancellationToken);
                lockedSystems = wandererData.Data.Systems
                    .Where(s => s.Locked)
                    .ToDictionary(s => s.SolarSystemId, s => s.Locked);
            }

            // Build the map recursively from home system
            var mapResult = DataHelpers.BuildWandererMapRecursive(_config.WandererHomeSystemId, signatures, wormholes);
            Console.WriteLine(
                $"Built map with {mapResult.Data.Connections.Count} connections and {mapResult.Data.Systems.Count} systems");

            // Apply locked status to systems
            foreach (var system in mapResult.Data.Systems)
                if (lockedSystems.TryGetValue(system.SolarSystemId, out var isLocked))
                    system.Locked = isLocked;

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

                // check if Polaris is on the map. if it is, skip syncing
                if (wandererData.Data.Systems.Any(s => s.SolarSystemId == 30000380))
                {
                    Console.WriteLine("Polaris detected on map - skipping sync");
                }
                else
                {
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
                    Console.WriteLine(
                        $"--- Deleting {deleteRequest.ConnectionIds.Count} connections and {deleteRequest.SystemIds.Count} systems from Wanderer ---");
                    await _wandererClient.DeleteSystemsAndConnectionsAsync(deleteRequest, cancellationToken);

                    // Post current systems and connections for Wanderer
                    var wandererResponse =
                        await _wandererClient.SubmitConnectionsAndSystemsAsync(mapResult, cancellationToken);
                    Console.WriteLine(
                        $"--- Added {wandererResponse.Data.Systems.Created} / Updated {wandererResponse.Data.Systems.Updated} systems ---");
                    Console.WriteLine(
                        $"--- Added {wandererResponse.Data.Connections.Created} / Updated {wandererResponse.Data.Connections.Updated} connections ---");
                }

                // Retrieve existing signatures from Wanderer
                // Console.WriteLine("\n--- Fetching existing signatures from Wanderer ---");
                // var existingSignatures = await _wandererClient.GetSignaturesAsync(cancellationToken);
                // Console.WriteLine($"Found {existingSignatures.Data.Count} existing signatures");

                // foreach (var existingSig in existingSignatures.Data)
                // {
                // 	// If signature no longer exists in Tripwire data, delete it from Wanderer
                // 	if (!signatures.Any())
                // 	{
                // 		Console.WriteLine($"Deleting signature {existingSig.EveId} from system {existingSig.SolarSystemId}");
                // 		await _wandererClient.DeleteSignatureAsync(existingSig.Id, cancellationToken);
                // 	}
                // }

                // Calculate required signatures
                // Console.WriteLine("\n--- Calculating required signatures ---");
                // var requiredSignatures = DataHelpers.CalculateRequiredSignatures(
                // 	mapResult,
                // 	signatures,
                // 	wormholes,
                // 	existingSignatures,
                // 	_config.WandererCharId.ToString());
                // Console.WriteLine($"Found {requiredSignatures.Count} new signatures to add");

                // Add signatures to Wanderer
                // if (requiredSignatures.Count > 0)
                // {
                // 	Console.WriteLine("\n--- Adding signatures to Wanderer ---");
                // 	var addedCount = 0;
                // 	foreach (var sigRequest in requiredSignatures)
                // 	{
                // 		try
                // 		{
                // 			await _wandererClient.AddSignatureAsync(sigRequest, cancellationToken);
                // 			addedCount++;
                // 		}
                // 		catch (Exception ex)
                // 		{
                // 			Console.WriteLine($"Warning: Failed to add signature {sigRequest.EveId} to system {sigRequest.SolarSystemId}: {ex.Message}");
                // 		}
                // 	}
                // 	Console.WriteLine($"Successfully added {addedCount} signatures");
                // }
            }
            else
            {
                Console.WriteLine("\n--- DEBUG MODE: Skipping Wanderer API calls ---");
                var mapResultJson =
                    JsonSerializer.Serialize(mapResult, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(mapResultJson);
            }
        }
        catch (OperationCanceledException ocex) when (cancellationToken.IsCancellationRequested)
        {
            // Only propagate real shutdown cancellations so the loop stops
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Sync cancelled. Message: {ocex.Message}");
            throw;
        }
        catch (OperationCanceledException ocex)
        {
            // Polly internal cancellation (e.g. timeout) - log and let the loop continue
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Sync interrupted (timeout/retry): {ocex.Message}");
        }
        catch (TimeoutRejectedException tex)
        {
            // Polly exhausted all retries due to timeout - log and let the loop continue
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Sync timed out after all retries: {tex.Message}");
        }
        catch (HttpRequestException hex)
        {
            // HTTP-level failure after all retries - log and let the loop continue
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Sync HTTP error after all retries: {hex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Sync failed: {ex}");
            throw;
        }
    }
}