using Content.Shared.Procedural;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

/// <summary>
/// Generates a dungeon based on a provided configuration and places it on specified coordinates.
/// Launched after grid ruins are spawned.
/// </summary>
[Prototype]
public sealed partial class LavalandDungeonRuinPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public ProtoId<DungeonConfigPrototype> Config;

    [DataField]
    public int SpawnAttempts = 8;

    [DataField(required: true)]
    public int Priority = int.MinValue;
}
