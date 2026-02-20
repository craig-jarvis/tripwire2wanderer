namespace Tripwire2Wanderer.WormHoleData;

public sealed class WormholeType
{
    public string Code { get; }
    public WHSize Size { get; }
    public long MaxJumpMass { get; }
    public long MaxStableMass { get; }
    public DestinationSpace Destination { get; }
    public int LifetimeHours { get; }
    public bool IsStatic { get; }

    public WormholeType(string code, WHSize size, long maxJumpMass, long maxStableMass,
                        DestinationSpace destination, int lifetimeHours, bool isStatic)
    {
        Code = code;
        Size = size;
        MaxJumpMass = maxJumpMass;
        MaxStableMass = maxStableMass;
        Destination = destination;
        LifetimeHours = lifetimeHours;
        IsStatic = isStatic;
    }
}

public static class Wormholes
{
    private static readonly Dictionary<string, WormholeType> _all =
        new Dictionary<string, WormholeType>(StringComparer.OrdinalIgnoreCase)
    {
        // Class 13 (Shattered)
        { "A009", new WormholeType("A009", WHSize.S, 5_000_000L, 500_000_000L, DestinationSpace.C13, 4, false) },

        // Known Space Connections
        { "A239", new WormholeType("A239", WHSize.L, 375_000_000L, 2_000_000_000L, DestinationSpace.LowSec, 24, false) },
        { "A641", new WormholeType("A641", WHSize.H, 1_000_000_000L, 2_000_000_000L, DestinationSpace.HighSec, 16, false) },
        { "B274", new WormholeType("B274", WHSize.L, 375_000_000L, 2_000_000_000L, DestinationSpace.HighSec, 24, false) },
        { "B449", new WormholeType("B449", WHSize.H, 1_000_000_000L, 2_000_000_000L, DestinationSpace.HighSec, 16, false) },
        { "B520", new WormholeType("B520", WHSize.XL, 2_000_000_000L, 3_300_000_000L, DestinationSpace.HighSec, 48, false) },
        { "E545", new WormholeType("E545", WHSize.L, 375_000_000L, 2_000_000_000L, DestinationSpace.NullSec, 16, false) },
        { "E587", new WormholeType("E587", WHSize.H, 1_000_000_000L, 3_000_000_000L, DestinationSpace.NullSec, 16, false) },
        { "Z060", new WormholeType("Z060", WHSize.M, 62_000_000L, 1_000_000_000L, DestinationSpace.NullSec, 24, false) },
        { "S199", new WormholeType("S199", WHSize.XL, 2_000_000_000L, 3_300_000_000L, DestinationSpace.NullSec, 16, false) },

        // Thera
        { "F135", new WormholeType("F135", WHSize.L, 375_000_000L, 750_000_000L, DestinationSpace.Thera, 16, false) },
        { "L031", new WormholeType("L031", WHSize.H, 1_000_000_000L, 2_000_000_000L, DestinationSpace.Thera, 16, false) },
        { "M164", new WormholeType("M164", WHSize.M, 62_000_000L, 2_000_000_000L, DestinationSpace.Thera, 16, false) },

        // Pochven
        { "F216", new WormholeType("F216", WHSize.H, 300_000_000L, 1_000_000_000L, DestinationSpace.Pochven, 16, false) },
        { "U372", new WormholeType("U372", WHSize.H, 300_000_000L, 1_000_000_000L, DestinationSpace.Pochven, 16, false) },

        // Drifter
        { "B735", new WormholeType("B735", WHSize.L, 375_000_000L, 750_000_000L, DestinationSpace.Drifter, 16, false) },
        { "C414", new WormholeType("C414", WHSize.L, 375_000_000L, 750_000_000L, DestinationSpace.Drifter, 16, false) },
        { "R051", new WormholeType("R051", WHSize.H, 1_000_000_000L, 3_000_000_000L, DestinationSpace.Drifter, 16, false) },
        { "S877", new WormholeType("S877", WHSize.L, 375_000_000L, 750_000_000L, DestinationSpace.Drifter, 16, false) },
        { "V928", new WormholeType("V928", WHSize.L, 375_000_000L, 750_000_000L, DestinationSpace.Drifter, 16, false) },

        // Wormhole Space Statics — C1
        { "E004", new WormholeType("E004", WHSize.S, 5_000_000L, 1_000_000_000L, DestinationSpace.C1, 4, true) },
        { "H121", new WormholeType("H121", WHSize.M, 62_000_000L, 500_000_000L, DestinationSpace.C1, 16, true) },
        { "P060", new WormholeType("P060", WHSize.M, 62_000_000L, 500_000_000L, DestinationSpace.C1, 16, true) },

        // Wormhole Space Statics — C2
        { "C125", new WormholeType("C125", WHSize.M, 62_000_000L, 1_000_000_000L, DestinationSpace.C2, 16, true) },
        { "D364", new WormholeType("D364", WHSize.L, 375_000_000L, 2_000_000_000L, DestinationSpace.C2, 16, true) },

        // Wormhole Space Statics — C3
        { "C247", new WormholeType("C247", WHSize.L, 375_000_000L, 2_000_000_000L, DestinationSpace.C3, 16, true) },

        // Wormhole Space Statics — C4
        { "E175", new WormholeType("E175", WHSize.L, 375_000_000L, 1_000_000_000L, DestinationSpace.C4, 16, true) },

        // Wormhole Space Statics — C5
        { "H296", new WormholeType("H296", WHSize.XL, 2_000_000_000L, 3_300_000_000L, DestinationSpace.C5, 24, true) },
        { "M555", new WormholeType("M555", WHSize.H, 1_000_000_000L, 3_000_000_000L, DestinationSpace.C5, 24, true) },

        // Wormhole Space Statics — C6
        { "A982", new WormholeType("A982", WHSize.L, 375_000_000L, 3_000_000_000L, DestinationSpace.C6, 24, true) },
        { "B041", new WormholeType("B041", WHSize.L, 375_000_000L, 5_000_000_000L, DestinationSpace.C6, 48, true) }
    };

    public static WormholeType Get(string code)
    {
        if (!_all.TryGetValue(code, out var type))
            throw new KeyNotFoundException($"Unknown wormhole type '{code}'.");
        return type;
    }

    public static IReadOnlyDictionary<string, WormholeType> All => _all;
}