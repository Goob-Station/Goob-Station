namespace Content.Pirate.Common.InteractionVerbs.Events;

/// <summary>
///     Base event for interaction verb attempts. Raised to determine if interaction should be allowed.
/// </summary>
[ByRefEvent]
public abstract class BaseInteractionVerbAttemptEvent : CancellableEntityEventArgs
{
    public bool Handled { get; set; } = false;
}
