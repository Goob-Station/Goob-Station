using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.MartialArts.Events;

/// <summary>
///     Raised whenever <see cref="IMoverComponent.CanMove"/> needs to be updated. Cancel this event to prevent a
///     mover from moving.
/// </summary>
public sealed class ComboAttackPerformedEvent(
    EntityUid performer,
    EntityUid target,
    EntityUid weapon,
    ComboAttackType type)
    : CancellableEntityEventArgs
{
    public EntityUid Performer { get; } = performer;
    public EntityUid Target { get; } = target;
    public EntityUid Weapon { get; } = weapon;
    public ComboAttackType Type { get; } = type;
}

[Serializable,NetSerializable]
public enum ComboAttackType : byte
{
    Harm,
    HarmLight,
    Disarm,
    Grab,
    Hug,
}
