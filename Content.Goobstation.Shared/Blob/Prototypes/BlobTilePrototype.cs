using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Blob.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob.Prototypes;

/// <summary>
/// Represents all static data needed for a blob tile, that should exist before the tile itself is spawned.
/// </summary>
[Prototype]
public sealed partial class BlobTilePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public EntProtoId<BlobTileComponent> SpawnId;

    [DataField]
    public FixedPoint2 Cost;

    /// <summary>
    /// If true, will connect this tile to the nearest node.
    /// Each node can have only 1 tile of this type.
    /// </summary>
    [DataField]
    public bool IsSpecial;

    [DataField]
    public bool Mutable = true;
}
