namespace Content.Goobstation.Shared.ModSuits;

/// <summary>
///     Event raises on modsuit when someone trying to toggle it
/// </summary>
public sealed class ToggleClothingAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid User { get; }
    public EntityUid Target { get; }

    public ToggleClothingAttemptEvent(EntityUid user, EntityUid target)
    {
        User = user;
        Target = target;
    }
}
