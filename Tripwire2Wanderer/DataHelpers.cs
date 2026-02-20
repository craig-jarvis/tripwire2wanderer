using Tripwire2Wanderer.Models;

namespace Tripwire2Wanderer;

public static class DataHelpers
{
    private const int MinValidSystemId = 10000;
    private const int GridSize = 15;

    public static (List<TripwireSignature>, List<TripwireWormhole>) GetTripwireSignaturesAndWormholesBySystemId(
        string systemId,
        List<TripwireSignature> signatures,
        List<TripwireWormhole> wormholes)
    {
        var filteredSignatures = signatures.Where(s => s.SystemId == systemId).ToList();
        var filteredWormholes = new List<TripwireWormhole>();

        foreach (var sig in filteredSignatures)
        foreach (var wh in wormholes)
            if (sig.Id == wh.InitialId || sig.Id == wh.SecondaryId)
                filteredWormholes.Add(wh);

        return (filteredSignatures, filteredWormholes);
    }

    public static WandererConnectionsAndSystemsEnvelope BuildWandererMapRecursive(
        int systemId,
        List<TripwireSignature> signatures,
        List<TripwireWormhole> wormholes)
    {
        var visited = new HashSet<string>();
        var systems = new List<WandererSystem>();
        var connections = new List<WandererConnection>();

        var systemIdStr = systemId.ToString();
        BuildMapRecursive(systemIdStr, signatures, wormholes, visited, systems, connections);

        return new WandererConnectionsAndSystemsEnvelope
        {
            Data = new WandererConnectionsAndSystems
            {
                Systems = systems,
                Connections = connections
            }
        };
    }

    private static void BuildMapRecursive(
        string systemId,
        List<TripwireSignature> signatures,
        List<TripwireWormhole> wormholes,
        HashSet<string> visited,
        List<WandererSystem> systems,
        List<WandererConnection> connections)
    {
        if (visited.Contains(systemId))
            return;

        visited.Add(systemId);

        var (filteredSigs, filteredWormholes) =
            GetTripwireSignaturesAndWormholesBySystemId(systemId, signatures, wormholes);

        if (filteredSigs.Count > 0)
            try
            {
                var system = NewWandererSystemFromTripwireSignature(filteredSigs[0]);
                if (system.SolarSystemId >= MinValidSystemId) systems.Add(system);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Warning: failed to create system from signature {filteredSigs[0].Id}: {ex.Message}");
            }

        foreach (var wormhole in filteredWormholes)
            try
            {
                var connection = NewWandererConnectionFromTripwireWormhole(wormhole, signatures);
                if (connection.SolarSystemSource == 0 || connection.SolarSystemTarget == 0)
                    continue;

                connections.Add(connection);

                string? otherSigId = null;
                foreach (var sig in filteredSigs)
                {
                    if (sig.Id == wormhole.InitialId)
                    {
                        otherSigId = wormhole.SecondaryId;
                        break;
                    }

                    if (sig.Id == wormhole.SecondaryId)
                    {
                        otherSigId = wormhole.InitialId;
                        break;
                    }
                }

                if (otherSigId == null)
                    continue;

                var targetSig = FindSignatureById(otherSigId, signatures);
                if (targetSig != null && targetSig.SystemId != "0" && targetSig.SystemId != null &&
                    !visited.Contains(targetSig.SystemId))
                    BuildMapRecursive(targetSig.SystemId, signatures, wormholes, visited, systems, connections);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: failed to create connection from wormhole {wormhole.Id}: {ex.Message}");
            }
    }

    public static WandererSystem NewWandererSystemFromTripwireSignature(TripwireSignature twSignature)
    {
        if (twSignature.SystemId?.Length < 5) throw new ArgumentException($"Invalid system ID: {twSignature.SystemId}");

        if (!int.TryParse(twSignature.SystemId, out var systemId))
            throw new ArgumentException($"Failed to parse system ID: {twSignature.SystemId}");

        return new WandererSystem
        {
            SolarSystemId = systemId,
            Visible = true
        };
    }

    public static WandererConnection NewWandererConnectionFromTripwireWormhole(
        TripwireWormhole twWormhole,
        List<TripwireSignature> signatures)
    {
        var connection = new WandererConnection();

        var sourceSig = FindSignatureById(twWormhole.InitialId, signatures);
        if (sourceSig == null)
            throw new ArgumentException($"Source signature not found: {twWormhole.InitialId}");

        if (sourceSig.SystemId?.Length < 5)
            return connection;

        if (!int.TryParse(sourceSig.SystemId, out var sourceSystemId))
            throw new ArgumentException($"Failed to parse source system ID: {sourceSig.SystemId}");

        connection.SolarSystemSource = sourceSystemId;

        var targetSig = FindSignatureById(twWormhole.SecondaryId, signatures);
        if (targetSig == null)
            throw new ArgumentException($"Target signature not found: {twWormhole.SecondaryId}");

        if (targetSig.SystemId is null)
        {
            connection.SolarSystemTarget = 0;
            return connection;
        }

        if (targetSig.SystemId?.Length < 5)
            return connection;

        if (!int.TryParse(targetSig.SystemId, out var targetSystemId))
            throw new ArgumentException($"Failed to parse target system ID: {targetSig.SystemId}");

        connection.SolarSystemTarget = targetSystemId;

        return connection;
    }

    public static TripwireSignature? FindSignatureById(string id, List<TripwireSignature> signatures)
    {
        return signatures.FirstOrDefault(s => s.Id == id);
    }

    public static bool HasChanges(
        WandererConnectionsAndSystemsEnvelope current,
        WandererConnectionsAndSystemsEnvelope newData)
    {
        var protectedSystems = BuildProtectedSystemsSet(current);

        // Build sets of system IDs for comparison
        var currentSystemIds = new HashSet<int>(current.Data.Systems.Select(s => s.SolarSystemId));
        var newSystemIds = new HashSet<int>(newData.Data.Systems.Select(s => s.SolarSystemId));

        // Check for added systems
        var addedSystems = newSystemIds.Except(currentSystemIds);
        if (addedSystems.Any())
            return true;

        // Check for removed systems (excluding protected ones)
        var removedSystems = currentSystemIds.Except(newSystemIds)
            .Where(id => !protectedSystems.Contains(id));
        if (removedSystems.Any())
            return true;

        // Build connection sets
        var currentConnections = new HashSet<(int, int)>(
            current.Data.Connections.Select(c => (c.SolarSystemSource, c.SolarSystemTarget)));
        var newConnections = new HashSet<(int, int)>(
            newData.Data.Connections.Select(c => (c.SolarSystemSource, c.SolarSystemTarget)));

        // Check for added connections
        var addedConnections = newConnections.Except(currentConnections);
        if (addedConnections.Any())
            return true;

        // Check for removed connections (excluding protected ones)
        var removedConnections = currentConnections.Except(newConnections)
            .Where(c => !protectedSystems.Contains(c.Item1) && !protectedSystems.Contains(c.Item2));
        if (removedConnections.Any())
            return true;

        // Check position changes for existing systems
        var commonSystemIds = currentSystemIds.Intersect(newSystemIds);
        var currentPositions = current.Data.Systems.ToDictionary(s => s.SolarSystemId, s => (s.PositionX, s.PositionY));
        var newPositions = newData.Data.Systems.ToDictionary(s => s.SolarSystemId, s => (s.PositionX, s.PositionY));

        foreach (var systemId in commonSystemIds)
        {
            var currentPos = currentPositions[systemId];
            var newPos = newPositions[systemId];

            if (Math.Abs(currentPos.PositionX - newPos.PositionX) > 0.01 ||
                Math.Abs(currentPos.PositionY - newPos.PositionY) > 0.01)
                return true;
        }

        return false;
    }


    public static WandererSystemAndConnectionsDeleteRequest CompareWandererEnvelopes(
        WandererConnectionsAndSystemsEnvelope current,
        WandererConnectionsAndSystemsEnvelope newData)
    {
        var deleteRequest = new WandererSystemAndConnectionsDeleteRequest();
        var protectedSystems = BuildProtectedSystemsSet(current);

        var newSystemMap = newData.Data.Systems
            .ToDictionary(s => s.SolarSystemId, s => true);

        foreach (var system in current.Data.Systems)
            if (!newSystemMap.ContainsKey(system.SolarSystemId))
                if (!protectedSystems.Contains(system.SolarSystemId))
                    deleteRequest.SystemIds.Add(system.SolarSystemId);

        var newConnectionMap = newData.Data.Connections
            .ToDictionary(c => (c.SolarSystemSource, c.SolarSystemTarget), c => true);

        foreach (var conn in current.Data.Connections)
        {
            var key = (conn.SolarSystemSource, conn.SolarSystemTarget);
            if (!newConnectionMap.ContainsKey(key))
                if (!protectedSystems.Contains(conn.SolarSystemSource) &&
                    !protectedSystems.Contains(conn.SolarSystemTarget))
                    deleteRequest.ConnectionIds.Add(conn.Id);
        }

        return deleteRequest;
    }

    public static WandererConnectionsAndSystemsEnvelope DedupWandererEnvelope(
        WandererConnectionsAndSystemsEnvelope envelope)
    {
        var deduped = new WandererConnectionsAndSystemsEnvelope
        {
            Data = new WandererConnectionsAndSystems()
        };

        var systemMap = new Dictionary<int, WandererSystem>();
        foreach (var system in envelope.Data.Systems)
            if (system.SolarSystemId != 0 && !systemMap.ContainsKey(system.SolarSystemId))
                systemMap[system.SolarSystemId] = system;

        deduped.Data.Systems = systemMap.Values.ToList();

        var connectionMap = new Dictionary<(int, int), WandererConnection>();
        foreach (var conn in envelope.Data.Connections)
        {
            var key = (conn.SolarSystemSource, conn.SolarSystemTarget);
            if (!connectionMap.ContainsKey(key)) connectionMap[key] = conn;
        }

        deduped.Data.Connections = connectionMap.Values.ToList();

        return deduped;
    }

    public static WandererConnectionsAndSystemsEnvelope CalculateSystemPositions(
        WandererConnectionsAndSystemsEnvelope envelope,
        int homeSystemId,
        double positionXSeparation,
        double positionYSeparation)
    {
        var result = envelope;

        var adjacency = new Dictionary<int, List<int>>();
        foreach (var conn in envelope.Data.Connections)
        {
            if (!adjacency.ContainsKey(conn.SolarSystemSource))
                adjacency[conn.SolarSystemSource] = new List<int>();
            if (!adjacency.ContainsKey(conn.SolarSystemTarget))
                adjacency[conn.SolarSystemTarget] = new List<int>();

            adjacency[conn.SolarSystemSource].Add(conn.SolarSystemTarget);
            adjacency[conn.SolarSystemTarget].Add(conn.SolarSystemSource);
        }

        var systemMap = result.Data.Systems.ToDictionary(s => s.SolarSystemId);

        if (!systemMap.ContainsKey(homeSystemId))
            return result;

        var (_, children) = BuildTreeStructure(adjacency, homeSystemId);

        var positions = CalculateTreePositions(homeSystemId, children, positionXSeparation, positionYSeparation);

        CenterMapAroundHomeSystem(positions, children, homeSystemId, positionYSeparation);

        foreach (var (systemId, pos) in positions)
            if (systemMap.TryGetValue(systemId, out var sys))
            {
                sys.PositionX = pos.x;
                sys.PositionY = pos.y;
            }

        return result;
    }

    private static (Dictionary<int, int>, Dictionary<int, List<int>>) BuildTreeStructure(
        Dictionary<int, List<int>> adjacency,
        int homeSystemId)
    {
        var visited = new HashSet<int>();
        var parent = new Dictionary<int, int>();
        var children = new Dictionary<int, List<int>>();
        var queue = new Queue<int>();

        queue.Enqueue(homeSystemId);
        visited.Add(homeSystemId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (!adjacency.ContainsKey(current))
                continue;

            foreach (var neighbor in adjacency[current])
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parent[neighbor] = current;

                    if (!children.ContainsKey(current))
                        children[current] = new List<int>();
                    children[current].Add(neighbor);

                    queue.Enqueue(neighbor);
                }
        }

        return (parent, children);
    }

    private static Dictionary<int, (double x, double y)> CalculateTreePositions(
        int homeSystemId,
        Dictionary<int, List<int>> children,
        double positionXSeparation,
        double positionYSeparation)
    {
        var positions = new Dictionary<int, (double x, double y)>();
        var nextYPosition = new Dictionary<int, double>();

        double CalculatePosition(int systemId, double depth)
        {
            var childList = children.ContainsKey(systemId) ? children[systemId] : new List<int>();
            var level = (int)depth;

            var y = nextYPosition.ContainsKey(level) ? nextYPosition[level] : 0.0;
            positions[systemId] = (depth, y);

            if (childList.Count == 0)
            {
                nextYPosition[level] = y + positionYSeparation;
                return positionYSeparation;
            }

            nextYPosition[(int)(depth + positionXSeparation)] = y;

            var totalChildHeight = 0.0;
            double firstChildY = 0.0, lastChildY = 0.0;

            for (var i = 0; i < childList.Count; i++)
            {
                var childId = childList[i];
                var childHeight = CalculatePosition(childId, depth + positionXSeparation);
                totalChildHeight += childHeight;

                if (i == 0)
                    firstChildY = positions[childId].y;
                lastChildY = positions[childId].y;
            }

            if (systemId != homeSystemId)
            {
                var centerY = (firstChildY + lastChildY) / 2.0;
                centerY = Math.Round(centerY / GridSize) * GridSize;
                positions[systemId] = (depth, centerY);
            }

            nextYPosition[level] = y + totalChildHeight;
            return totalChildHeight;
        }

        CalculatePosition(homeSystemId, 0);
        return positions;
    }

    private static void CenterMapAroundHomeSystem(
        Dictionary<int, (double x, double y)> positions,
        Dictionary<int, List<int>> children,
        int homeSystemId,
        double positionYSeparation)
    {
        if (!children.ContainsKey(homeSystemId) || children[homeSystemId].Count == 0)
            return;

        var childrenIds = children[homeSystemId];

        // If there's only one child, it should be at the same Y as home system (0)
        if (childrenIds.Count == 1)
        {
            var childId = childrenIds[0];
            var childPos = positions[childId];
            var offset = -childPos.y;

            // Apply offset to all systems
            var updatedPositions = new Dictionary<int, (double x, double y)>();
            foreach (var (systemId, pos) in positions)
                if (systemId != homeSystemId)
                    updatedPositions[systemId] = (pos.x, pos.y + offset);
                else
                    updatedPositions[systemId] = (0, 0);

            positions.Clear();
            foreach (var (k, v) in updatedPositions) positions[k] = v;

            return;
        }

        // Calculate the center Y of all direct children of the home system
        var firstChildY = positions[childrenIds[0]].y;
        var lastChildY = positions[childrenIds[childrenIds.Count - 1]].y;
        var childrenCenterY = (firstChildY + lastChildY) / 2.0;

        // Calculate offset needed to center children around home system at (0, 0)
        // Round to nearest multiple of positionYSeparation to maintain grid alignment
        var yOffset = -Math.Round(childrenCenterY / positionYSeparation) * positionYSeparation;

        // Apply offset to all systems except home system
        var updatedPositions2 = new Dictionary<int, (double x, double y)>();
        foreach (var (systemId, pos) in positions)
            if (systemId != homeSystemId)
                updatedPositions2[systemId] = (pos.x, pos.y + yOffset);
            else
                // Home system stays at (0, 0)
                updatedPositions2[systemId] = (0, 0);

        positions.Clear();
        foreach (var (k, v) in updatedPositions2) positions[k] = v;
    }


    public static string ConvertTripwireSigIdToEveId(string sigId)
    {
        if (sigId == "???")
            return string.Empty;

        if (sigId.Length != 6)
            throw new ArgumentException("Invalid signature id");

        var letters = sigId.Substring(0, 3).ToUpper();
        var numbers = sigId.Substring(3);

        return $"{letters}-{numbers}";
    }

    public static WandererSignatureRequest NewWandererSignatureRequestFromTripwireSignature(
        TripwireSignature twSignature, List<TripwireWormhole> wormholes, List<TripwireSignature> allSignatures,
        string characterEveId)
    {
        return WandererSignatureRequest.FromTripwireSignature(twSignature, wormholes, allSignatures, characterEveId);
    }

    public static List<WandererSignatureRequest> CalculateRequiredSignatures(
        WandererConnectionsAndSystemsEnvelope mapData,
        List<TripwireSignature> tripwireSignatures,
        List<TripwireWormhole> tripwireWormholes,
        WandererSignatureEnvelope existingSignatures,
        string characterEveId)
    {
        var requiredSignatures = new List<WandererSignatureRequest>();

        // Build a set of system IDs that are in the map
        var systemIdsInMap = new HashSet<int>(mapData.Data.Systems.Select(s => s.SolarSystemId));

        // Build a set of existing signature identifiers (solar_system_id + eve_id)
        var existingSigKeys = new HashSet<(int systemId, string? eveId)>();
        foreach (var sig in existingSignatures.Data)
            if (!sig.Deleted && sig.SolarSystemId != 0 && !string.IsNullOrEmpty(sig.EveId))
                existingSigKeys.Add((sig.SolarSystemId, sig.EveId));

        // Process each Tripwire signature
        foreach (var twSig in tripwireSignatures)
        {
            // Parse the system ID
            if (!int.TryParse(twSig.SystemId, out var systemId) || systemId == 0)
                continue;

            // Only include signatures for systems that are in the map
            if (!systemIdsInMap.Contains(systemId))
                continue;

            // Create the signature request to get the parsed eve_id
            var sigRequest =
                WandererSignatureRequest.FromTripwireSignature(twSig, tripwireWormholes, tripwireSignatures,
                    characterEveId);

            // Skip if we couldn't parse a valid eve_id
            if (string.IsNullOrEmpty(sigRequest.EveId))
                continue;

            // Check if this signature already exists in Wanderer
            var key = (sigRequest.SolarSystemId, sigRequest.EveId);
            if (existingSigKeys.Contains(key))
                continue;

            // This is a new signature that needs to be added
            requiredSignatures.Add(sigRequest);
        }

        return requiredSignatures;
    }

    private static HashSet<int> BuildProtectedSystemsSet(
        WandererConnectionsAndSystemsEnvelope current)
    {
        // Build adjacency list from current connections
        var adjacency = new Dictionary<int, List<int>>();
        foreach (var conn in current.Data.Connections)
        {
            if (!adjacency.ContainsKey(conn.SolarSystemSource))
                adjacency[conn.SolarSystemSource] = new List<int>();
            if (!adjacency.ContainsKey(conn.SolarSystemTarget))
                adjacency[conn.SolarSystemTarget] = new List<int>();

            adjacency[conn.SolarSystemSource].Add(conn.SolarSystemTarget);
            adjacency[conn.SolarSystemTarget].Add(conn.SolarSystemSource);
        }

        // Find all systems connected to locked systems
        var protectedSystems = new HashSet<int>();
        foreach (var system in current.Data.Systems)
            if (system.Locked)
            {
                // BFS to find all connected systems
                var queue = new Queue<int>();
                var visited = new HashSet<int>();

                queue.Enqueue(system.SolarSystemId);
                visited.Add(system.SolarSystemId);

                while (queue.Count > 0)
                {
                    var currentId = queue.Dequeue();
                    protectedSystems.Add(currentId);

                    if (adjacency.ContainsKey(currentId))
                        foreach (var neighbor in adjacency[currentId])
                            if (!visited.Contains(neighbor))
                            {
                                visited.Add(neighbor);
                                queue.Enqueue(neighbor);
                            }
                }
            }

        return protectedSystems;
    }
}