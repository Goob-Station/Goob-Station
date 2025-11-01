using Content.Goobstation.Shared.Blob.Prototypes;
using Content.Shared.Actions;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Blob.Events;

/// <summary>
/// Tries to transform the Target blob tile in other type, making checks for Node and/or similar tiles.
/// </summary>
public sealed partial class BlobTransformTileActionEvent : WorldTargetActionEvent
{
    /// <summary>
    /// Type of tile that can be transformed.
    /// Will be ignored if equals to Invalid.
    /// </summary>
    [DataField]
    public ProtoId<BlobTilePrototype> TransformFrom;

    /// <summary>
    /// Type of the resulting tile.
    /// </summary>
    [DataField]
    public ProtoId<BlobTilePrototype> TileType;

    /// <summary>
    /// If specified, tries to find a blob node
    /// in given radius and returns back if failed.
    /// </summary>
    [DataField]
    public float? NodeSearchRadius;

    /// <summary>
    /// If specified, tries to find a tile of the same type
    /// in given radius and returns back if failed.
    /// </summary>
    [DataField]
    public float? TileSearchRadius;

    public BlobTransformTileActionEvent(
        EntityUid performer,
        EntityCoordinates target,
        ProtoId<BlobTilePrototype> transformFrom,
        ProtoId<BlobTilePrototype> tileType)
    {
        Performer = performer;
        Target = target;
        TransformFrom = transformFrom;
        TileType = tileType;
    }
}

/// <summary>
/// Raised after the blob tile was successfully transformed.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class BlobTileTransformedEvent : EntityEventArgs;

public sealed partial class BlobCreateBlobbernautActionEvent : WorldTargetActionEvent;

public sealed partial class BlobSplitCoreActionEvent : EntityTargetActionEvent
{
    [DataField]
    public EntProtoId CoreProtoId;
}

public sealed partial class BlobSwapCoreActionEvent : EntityTargetActionEvent;
public sealed partial class BlobToCoreActionEvent : InstantActionEvent;
public sealed partial class BlobSwapChemActionEvent : InstantActionEvent;
