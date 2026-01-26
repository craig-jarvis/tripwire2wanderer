using System.Text.Json;
using Tripwire2Wanderer;
using Tripwire2Wanderer.Clients;

// Parse command line arguments
var debug = args.Contains("--debug");

if (debug)
{
    Console.WriteLine("Running in DEBUG mode - Wanderer API calls will be skipped");
}

// Setup cancellation token for graceful shutdown
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("\nShutdown signal received, stopping...");
    e.Cancel = true;
    cts.Cancel();
};

try
{
    // Load configuration from .env and environment variables
    var config = Config.Load();
    
    Console.WriteLine($"Poll interval: {config.PollIntervalSeconds} seconds");
    Console.WriteLine("Starting sync loop...\n");

    while (!cts.Token.IsCancellationRequested)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            await RunSyncAsync(config, debug, cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during sync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        var elapsed = DateTime.UtcNow - startTime;
        Console.WriteLine($"\nSync completed in {elapsed.TotalSeconds:F2} seconds");
        
        // Wait for the configured interval before next sync
        var waitTime = TimeSpan.FromSeconds(config.PollIntervalSeconds) - elapsed;
        if (waitTime > TimeSpan.Zero && !cts.Token.IsCancellationRequested)
        {
            Console.WriteLine($"Waiting {waitTime.TotalSeconds:F0} seconds until next sync...\n");
            await Task.Delay(waitTime, cts.Token);
        }
    }
    
    Console.WriteLine("Shutdown complete");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Application stopped");
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    Environment.Exit(1);
}

static async Task RunSyncAsync(Config config, bool debug, CancellationToken cancellationToken)
{
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Starting sync...");
    
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

    // Build the map recursively from home system
    var mapResult = DataHelpers.BuildWandererMapRecursive(config.WandererHomeSystemId, signatures, wormholes);
    Console.WriteLine(
        $"Built map with {mapResult.Data.Connections.Count} connections and {mapResult.Data.Systems.Count} systems");

    mapResult = DataHelpers.DedupWandererEnvelope(mapResult);
    mapResult = DataHelpers.CalculateSystemPositions(
        mapResult,
        config.WandererHomeSystemId,
        config.PositionXSeparation,
        config.PositionYSeparation);

    if (!debug)
    {
        // Get current systems and connections from Wanderer
        Console.WriteLine("\n--- Fetching current Wanderer data ---");
        var wandererData = await wClient.GetSystemsAndConnectionsAsync();

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
        await wClient.DeleteSystemsAndConnectionsAsync(deleteRequest);

        // Post current systems and connections for Wanderer
        var wandererResponse = await wClient.SubmitConnectionsAndSystemsAsync(mapResult);
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