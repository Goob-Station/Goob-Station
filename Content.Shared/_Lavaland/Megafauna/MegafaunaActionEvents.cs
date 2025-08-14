using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna;

public sealed partial class SpawnEntityActionEvent : WorldTargetActionEvent
{
    [DataField(required: true)]
    public EntProtoId Spawn;

    /// <summary>
    /// If true, will attach spawned entity to the target.
    /// </summary>
    [DataField]
    public bool AttachToTarget;
}

/// <summary>
/// Raised on a spawned entity by <see cref="SpawnEntityActionEvent"/>.
/// </summary>
[ByRefEvent]
public readonly record struct SpawnedByActionEvent(EntityUid User, EntityUid? Target);

public sealed partial class MegafaunaBlinkActionEvent : WorldTargetActionEvent
{
    /// <summary>
    /// Entity that will be spawned on a target blink position.
    /// </summary>
    [DataField]
    public EntProtoId? SpawnOnTarget;

    /// <summary>
    /// Entity that will be spawned on original position when before teleportation.
    /// </summary>
    [DataField]
    public EntProtoId? SpawnOnUsed;

    [DataField]
    public TimeSpan Duration;

    [DataField]
    public SoundSpecifier? Sound;
}

public sealed partial class ToggleTileMovementActionEvent : EntityTargetActionEvent;
