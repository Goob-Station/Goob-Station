using Content.Shared.InteractionVerbs;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     An action that raises an event on the target or the user. Made for interop with systems that rely on events.
/// </summary>
[Serializable]
public sealed partial class RaiseEventAction : InteractionAction
{
    /// <summary>
    ///     The event to raise. Must be serializable because it will be copied before being raised.
    /// </summary>
    /// <remarks>
    ///     If this is a handled event, the result of the action is whether the event was handled.
    ///     Likewise, if it's cancellable, the result is whether it was not cancelled.
    /// </remarks>
    [DataField("event", required: true)]
    public EntityEventArgs? EventData;

    /// <summary>
    ///     If true, the event will be raised on the user. Otherwise, it will be raised on the target.
    /// </summary>
    [DataField]
    public bool OnUser = false;

    [DataField]
    public bool Broadcast = false;

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool beforeDelay, VerbDependencies deps)
    {
        return true;
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        if (EventData is null)
            return false;

        var ev = deps.Serialization.CreateCopy(EventData, notNullableOverride: true);
        deps.EntMan.EventBus.RaiseLocalEvent(OnUser ? args.User : args.Target, ev, Broadcast);

        if (ev is HandledEntityEventArgs handled)
            return handled.Handled;
        if (ev is CancellableEntityEventArgs cancellable)
            return !cancellable.Cancelled;

        return true;
    }
}
