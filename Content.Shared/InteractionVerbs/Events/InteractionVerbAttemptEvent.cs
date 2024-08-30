namespace Content.Shared.InteractionVerbs.Events;

/// <summary>
///     Raised directly on the performer of the interaction verb and on its target to determine if it should be allowed.
///     Note that this is raised if and only if verb's own CanPerform check returns true.
/// </summary>
[ByRefEvent]
public sealed class InteractionVerbAttemptEvent(InteractionVerbPrototype proto, InteractionArgs args) : CancellableEntityEventArgs
{
    public bool Handled { get; set; } = false;

    public InteractionVerbPrototype Proto => proto;
    public InteractionArgs Args => args;
}
