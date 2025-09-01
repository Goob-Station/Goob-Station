using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Plasma;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;

namespace Content.Shared._White.Actions;

public sealed class PlasmaCostActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPlasmaSystem _plasma = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlasmaCostActionComponent, ActionRelayedEvent<PlasmaAmountChangeEvent>>(OnPlasmaAmountChange);
        SubscribeLocalEvent<PlasmaCostActionComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    private void OnPlasmaAmountChange(EntityUid uid, PlasmaCostActionComponent component, ActionRelayedEvent<PlasmaAmountChangeEvent> args)
    {
        _actions.SetEnabled(uid, component.PlasmaCost <= args.Args.Amount);
    }

    private void OnActionPerformed(EntityUid uid, PlasmaCostActionComponent component, ActionPerformedEvent args)
    {
        if (component.ShouldChangePlasma)
            _plasma.ChangePlasmaAmount(args.Performer, -component.PlasmaCost);
    }
}
