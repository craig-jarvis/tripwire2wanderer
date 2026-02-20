namespace Tripwire2Wanderer.WormHoleData;

public sealed class WHType
{
    public string Code { get; }
    public WHSize Size { get; }
    public long MaxJumpMass { get; }
    public long MaxStableMass { get; }
    public DestinationSpace Destination { get; }
    public int LifetimeHours { get; }
    public bool IsStatic { get; }

    public WHType(
        string code,
        WHSize size,
        long maxJumpMass,
        long maxStableMass,
        DestinationSpace destination,
        int lifetimeHours,
        bool isStatic = true)
    {
        Code = code;
        Size = size;
        MaxJumpMass = maxJumpMass;
        MaxStableMass = maxStableMass;
        Destination = destination;
        LifetimeHours = lifetimeHours;
        IsStatic = isStatic;
    }

    public long MinTotalMass => (long)(MaxStableMass * 0.9);
    public long MaxTotalMass => (long)(MaxStableMass * 1.1);
}
