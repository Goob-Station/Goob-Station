using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.MartialArts.Events;

/// <summary>
///     Raised when a martial arts combo attack is performed. Contains information about
///     the performer, target, weapon used, and the type of combo attack.
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
