using Robust.Shared.GameObjects;

namespace Content.Pirate.Shared.Aiming.Events;

/// <summary>
/// Raised when an entity that is being aimed at moves.
/// </summary>
public sealed class OnAimingTargetMoveEvent : EntityEventArgs
{
    public readonly EntityUid Target;

    public OnAimingTargetMoveEvent(EntityUid target)
    {
        Target = target;
    }

}
