namespace Content.Shared.Goobstation.Interaction;

/// <summary>
///     UseAttempt, but for item.
/// </summary>
public sealed class UseInHandAttemptEvent(EntityUid user) : CancellableEntityEventArgs
{
    public EntityUid User { get; } = user;
}
