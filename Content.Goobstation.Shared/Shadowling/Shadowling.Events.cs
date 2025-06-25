namespace Content.Shared._EE.Shadowling;

/// <summary>
/// Raised when a shadowling ascends. For round-end text.
/// </summary>
public sealed class ShadowlingAscendEvent : EntityEventArgs
{

}

/// <summary>
/// Raised when a shadowling dies. For ending their antag-ness.
/// </summary>
public sealed class ShadowlingDeathEvent : EntityEventArgs
{

}
