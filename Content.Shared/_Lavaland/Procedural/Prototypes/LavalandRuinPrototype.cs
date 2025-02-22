using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

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
}
