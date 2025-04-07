using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

/// <summary>
/// Contains information about Lavaland ruin configuration.
/// </summary>
[Prototype]
public sealed partial class LavalandDungeonRuinPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField(required: true)]
    public Vector2i Boundary { get; }

    [DataField(required: true)]
    public EntProtoId SpawnedMarker;

    [DataField]
    public int SpawnAttemps = 8;

    [DataField(required: true)]
    public int Priority = int.MinValue;
}
