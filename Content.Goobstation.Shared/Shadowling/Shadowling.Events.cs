namespace Content.Goobstation.Shared.Shadowling;

/// <summary>
/// Raised when a shadowling ascends. For round-end text.
/// </summary>
public sealed class ShadowlingAscendEvent(EntityUid ascended) : EntityEventArgs
{
    public EntityUid ShadowlingAscended = ascended;
}

/// <summary>
/// Raised when a shadowling dies. For ending their antag-ness.
/// </summary>
public sealed class ShadowlingDeathEvent : EntityEventArgs;
