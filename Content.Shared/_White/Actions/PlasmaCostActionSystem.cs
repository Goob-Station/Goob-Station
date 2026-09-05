using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._White.Xenomorphs.Plasma;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Popups;

namespace Content.Shared._White.Actions;

public sealed class PlasmaCostActionSystem : EntitySystem
{
    [Dependency] private readonly SharedPlasmaSystem _plasma = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlasmaCostActionComponent, ActionAttemptEvent>(OnActionAttempt); // Goobstation
    }

    /// <summary>
    /// Checks if the performer has enough plasma for the action.
    /// Returns true if the action should proceed, false if it should be blocked.
    /// Goobstation
    /// </summary>
    public bool HasEnoughPlasma(EntityUid performer, FixedPoint2 cost)
    {
        if (cost <= 0)
            return true;

        return TryComp<PlasmaVesselComponent>(performer, out var plasmaVessel) &&
               plasmaVessel.Plasma >= cost;
    }

    /// <summary>
    /// Deducts plasma from the performer. Call this after confirming the action succeeds.
    /// </summary>
    public void DeductPlasma(EntityUid performer, FixedPoint2 cost)
    {
        if (cost > 0)
            _plasma.ChangePlasmaAmount(performer, -cost);
    }

    private void OnActionAttempt(Entity<PlasmaCostActionComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!_plasma.HasPlasma(args.User, ent.Comp.PlasmaCost))
        {
            _popup.PopupPredicted(Loc.GetString("plasma-not-enough"), args.User, args.User);
            args.Cancelled = true;
            return;
        }
    }
    // Goobstation end
}
