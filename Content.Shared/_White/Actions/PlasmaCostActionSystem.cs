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
        SubscribeLocalEvent<PlasmaCostActionComponent, ActionAttemptEvent>(OnActionAttempt); // Goobstation
        SubscribeLocalEvent<PlasmaCostActionComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    private void OnPlasmaAmountChange(EntityUid uid, PlasmaCostActionComponent component, ActionRelayedEvent<PlasmaAmountChangeEvent> args)
    {
        _actions.SetEnabled(uid, component.PlasmaCost <= args.Args.Amount);
    }

    // <Goobstation>
    private void OnActionAttempt(Entity<PlasmaCostActionComponent> ent, ref ActionAttemptEvent args)
    {
        if (!_plasma.HasPlasma(args.User, ent.Comp.PlasmaCost))
            args.Cancelled = true;
    }
    // </Goobstation>

    private void OnActionPerformed(EntityUid uid, PlasmaCostActionComponent component, ActionPerformedEvent args)
    {
        if (component.ShouldChangePlasma)
            _plasma.ChangePlasmaAmount(args.Performer, -component.PlasmaCost);
    }
}
