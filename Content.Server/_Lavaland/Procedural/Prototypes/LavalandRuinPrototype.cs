using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Procedural.Prototypes;

/// <summary>
/// Contains information about Lavaland ruin configuration.
/// </summary>
[Prototype]
public sealed partial class LavalandRuinPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField] public string Name = "Unknown Ruin";

    [DataField(required: true)]
    public string Path { get; } = default!;

    /// <summary>
    /// Higher this value is, the more further away from the outpost
    /// ruin will spawn and more rare it will be.
    /// </summary>
    [DataField] public ushort Weight = 1;


}
