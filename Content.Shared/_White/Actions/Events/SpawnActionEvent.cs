using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._White.Actions.Events;

/// <summary>
/// Event for placing a tile and/or spawning an entity in under the object that triggers it with a delay.
/// </summary>
public sealed partial class SpawnTileEntityActionEvent : InstantActionEvent
{
    /// <summary>
    /// The prototype of the entity to be created
    /// </summary>
    [DataField]
    public EntProtoId? Entity;

    /// <summary>
    /// The identifier of the tile to be placed
    /// </summary>
    [DataField]
    public string? TileId;

    /// <summary>
    /// The sound that will be played when the action is performed
    /// </summary>
    [DataField]
    public SoundSpecifier? Audio;

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int BlockedCollisionMask;

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionLayer>))]
    public int BlockedCollisionLayer;
}

/// <summary>
/// Event for placing a tile and/or spawning an entity at a specified position on the map with a delay.
/// </summary>
public sealed partial class PlaceTileEntityEvent : WorldTargetActionEvent
{
    /// <summary>
    /// The prototype of the entity to be created
    /// </summary>
    [DataField]
    public EntProtoId? Entity;

    /// <summary>
    /// The identifier of the tile to be placed
    /// </summary>
    [DataField]
    public string? TileId;

    /// <summary>
    /// The sound that will be played when the action is performed
    /// </summary>
    [DataField]
    public SoundSpecifier? Audio;

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int BlockedCollisionMask;

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionLayer>))]
    public int BlockedCollisionLayer;

    /// <summary>
    /// The duration of the action in seconds
    /// </summary>
    [DataField]
    public float Length;
}

[Serializable, NetSerializable]
public sealed partial class PlaceTileEntityDoAfterEvent : DoAfterEvent
{
    public NetCoordinates Target;

    public EntProtoId? Entity;

    public string? TileId;

    public SoundSpecifier? Audio;

    public int BlockedCollisionMask;

    public int BlockedCollisionLayer;

    public override DoAfterEvent Clone() => this;
}
