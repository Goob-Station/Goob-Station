using Content.Shared._White.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;

namespace Content.Shared._White.Actions;

public sealed class ActionRelaySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ActionsComponent, PlasmaAmountChangeEvent>(RelayEvent);
    }

    public void RelayEvent<T>(EntityUid uid, ActionsComponent component, T args) where T : EntityEventArgs
    {
        var ev = new ActionRelayedEvent<T>(args);
        var actions = _actions.GetActions(uid, component);
        foreach (var action in actions)
        {
            RaiseLocalEvent(action.Owner, ev);
        }
    }
}

public sealed class ActionRelayedEvent<TEvent>(TEvent args) : EntityEventArgs
{
    public TEvent Args = args;
}
